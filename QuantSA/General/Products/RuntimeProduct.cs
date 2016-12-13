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
        public static Product CreateFromScript(String filename)
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

        private static string Expand(string productName, string sourceCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using QuantSA.Valuation;");
            sb.AppendLine("using QuantSA.General; ");
            sb.AppendLine("using System; ");
            sb.AppendLine("using System.Collections.Generic; ");
            sb.AppendLine("public class " + productName + " : ProductWrapper");
            sb.AppendLine("{");
            sb.AppendLine(sourceCode);
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
