using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QuantSA.General
{
    [Serializable]
    public class RuntimeProduct
    {
        /// <summary>
        /// Processes compile errors and makes them into a readable string.
        /// </summary>
        /// <param name="results">The compiler results.</param>
        /// <exception cref="System.Exception">
        /// Assembly must only define one type : A Class that extends QuantSA.General.Product.
        /// </exception>
        private static void ProcessErrors(CompilerResults results)
        {
            if (results.Errors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                foreach (CompilerError compErr in results.Errors)
                    errorMessage.Append("Line number " + compErr.Line +
                                        ", Error Number: " + compErr.ErrorNumber +
                                        ", '" + compErr.ErrorText + ";" +
                                        Environment.NewLine + Environment.NewLine);
                throw new Exception(errorMessage.ToString());
            }

            if (results.CompiledAssembly.DefinedTypes.Count() > 1)
                throw new Exception(
                    "Assembly must only define one type : A Class that extends QuantSA.General.Product.");
        }

        /// <summary>
        /// Creates a <see cref="RuntimeProduct"/> from a source listing in a file.  The file must only
        /// contain the class fields and a GetCFs method.
        /// </summary>
        /// <param name="filename">The filename of the script with the full path.</param>
        /// <returns></returns>        
        public static Product CreateFromScript(string filename)
        {
            var sourceCode = File.ReadAllText(filename);
            var productName = Path.GetFileNameWithoutExtension(filename);

            return CreateFromString(productName, sourceCode);
        }

        /// <summary>
        /// Creates a <see cref="RuntimeProduct"/> from a string containing C# source code.  The file must only
        /// contain the class fields and a GetCFs method.  If a full implementation of the product is required 
        /// rather use <see cref="CreateFromSourceFile(string)"/>
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The defined type must derive from QuantSA.General.Product</exception>
        public static Product CreateFromString(string productName, string sourceCode)
        {
            var codeProvider = CodeDomProvider.CreateProvider("CSharp");
            var parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            UpdateReferencedAssemblies(parameters, folder);

            var expandedSourceCode = Expand(productName, sourceCode);

            var results = codeProvider.CompileAssemblyFromSource(parameters, expandedSourceCode);
            ProcessErrors(results);

            var typeName = results.CompiledAssembly.DefinedTypes.First().Name;
            var productType = results.CompiledAssembly.GetType(typeName);
            if (!typeof(Product).IsAssignableFrom(productType))
                throw new Exception("The defined type must derive from QuantSA.General.Product");

            return (Product) Activator.CreateInstance(productType);
        }

        private static void UpdateReferencedAssemblies(CompilerParameters parameters, string folder)
        {
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.Shared.dll"));
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.Core.dll"));
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.Valuation.dll"));
        }

        /// <summary>
        /// Expands the specified product by turning it into a full C# class file with usings at the top 
        /// and inserts a clone method.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <returns></returns>
        private static string Expand(string productName, string sourceCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using QuantSA.Shared;");
            sb.AppendLine("using QuantSA.Shared.Dates;");
            sb.AppendLine("using QuantSA.Shared.MarketObservables;");
            sb.AppendLine("using QuantSA.Shared.Primitives;");
            sb.AppendLine("using QuantSA.Core.Products;");
            sb.AppendLine("public class " + productName + " : ProductWrapper");
            sb.AppendLine("{");
            sb.AppendLine(sourceCode);
            sb.AppendLine("public " + productName + " ()");
            sb.AppendLine("{ Init(); }");
            sb.AppendLine("public override IProduct Clone()");
            sb.AppendLine("{");
            sb.AppendLine(productName + " product = new " + productName + "(); ");
            sb.AppendLine("if (valueDate != null)");
            sb.AppendLine("    product.valueDate = new Date(valueDate);");
            sb.AppendLine("return product;");
            sb.AppendLine("}");

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}