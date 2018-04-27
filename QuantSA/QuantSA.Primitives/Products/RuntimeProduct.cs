using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    [Serializable]
    public class RuntimeProduct
    {
        /// <summary>
        /// Creates a from a full source file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The defined type must derive from QuantSA.General.Product</exception>
        public static Product CreateFromSourceFile(string filename)
        {
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.General.dll"));
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.Valuation.dll"));

            CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, new string[] { filename });
            ProcessErrors(results);

            string typeName = results.CompiledAssembly.DefinedTypes.First().Name;
            Type productType = results.CompiledAssembly.GetType(typeName);
            if (!typeof(Product).IsAssignableFrom(productType))
            {
                throw new Exception("The defined type must derive from QuantSA.General.Product");
            }

            return (Product)Activator.CreateInstance(productType);
        }

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
                StringBuilder errorMessage = new StringBuilder();
                foreach (CompilerError CompErr in results.Errors)
                {
                    errorMessage.Append("Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine);
                }
                throw new Exception(errorMessage.ToString());
            }
            if (results.CompiledAssembly.DefinedTypes.Count() > 1)
            {
                throw new Exception("Assembly must only define one type : A Class that extends QuantSA.General.Product.");
            }
        }

        /// <summary>
        /// Creates a <see cref="RuntimeProduct"/> from a source listing in a file.  The file must only
        /// contain the class fields and a GetCFs method.  If a full implementation of the product is required 
        /// rather use <see cref="CreateFromSourceFile(string)"/>
        /// </summary>
        /// <param name="filename">The filename of the script with the full path.</param>
        /// <returns></returns>        
        public static Product CreateFromScript(string filename)
        {
            string sourceCode = File.ReadAllText(filename);
            string productName = Path.GetFileNameWithoutExtension(filename);

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
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.General.dll"));
            parameters.ReferencedAssemblies.Add(Path.Combine(folder, "QuantSA.Valuation.dll"));

            string expandedSourceCode = Expand(productName, sourceCode);

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, new string[] { expandedSourceCode });
            ProcessErrors(results);

            string typeName = results.CompiledAssembly.DefinedTypes.First().Name;
            Type productType = results.CompiledAssembly.GetType(typeName);
            if (!typeof(Product).IsAssignableFrom(productType))
            {
                throw new Exception("The defined type must derive from QuantSA.General.Product");
            }

            return (Product)Activator.CreateInstance(productType);
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using QuantSA.Valuation;");
            sb.AppendLine("using QuantSA.General; ");
            sb.AppendLine("using QuantSA.Primitives.Dates; ");
            sb.AppendLine("using System; ");
            sb.AppendLine("using System.Collections.Generic; ");
            sb.AppendLine("public class " + productName + " : ProductWrapper");
            sb.AppendLine("{");
            sb.AppendLine(sourceCode);
            sb.AppendLine("public " + productName + " ()");
            sb.AppendLine("{ Init(); }");
            sb.AppendLine("public override Product Clone()");
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
