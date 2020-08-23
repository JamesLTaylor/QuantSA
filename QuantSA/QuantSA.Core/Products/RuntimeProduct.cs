using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using QuantSA.Core.Primitives;

namespace QuantSA.Core.Products
{
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
        /// contain the class fields and a GetCFs method."/>
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The defined type must derive from QuantSA.General.Product</exception>
        public static Product CreateFromString(string productName, string sourceCode)
        {
            var expandedSourceCode = Expand(productName, sourceCode);
            var assembly = RoslynCompile(expandedSourceCode);

            var typeName = assembly.DefinedTypes.First().Name;
            var productType = assembly.GetType(typeName);
            if (!typeof(Product).IsAssignableFrom(productType))
                throw new Exception("The defined type must derive from QuantSA.General.Product");

            return (Product) Activator.CreateInstance(productType);
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

        /// <summary>
        /// Get an assembly that is compiled from <paramref name="codeToCompile"/>.
        /// 
        /// Based off https://stackoverflow.com/a/51128354 and
        /// https://github.com/joelmartinez/dotnet-core-roslyn-sample/blob/master/Program.cs
        /// </summary>
        /// <param name="codeToCompile"></param>
        /// <returns></returns>
        private static Assembly RoslynCompile(string codeToCompile)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            string assemblyName = Path.GetRandomFileName();
            var refPaths = new[] {
                GetAssemblyByName("System.Private.CoreLib"),
                GetAssemblyByName("System.Runtime"),
                GetAssemblyByName("System.Collections"),
                GetAssemblyByName("QuantSA.Shared"),
                GetAssemblyByName("QuantSA.Core"),
                GetAssemblyByName("netstandard")
            };
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Assembly assembly = null;
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                }
            }

            return assembly;
        }

        private static string GetAssemblyByName(string name)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == name);
            if (assembly == null)
                throw new FileNotFoundException($"Could not find assembly {name}. Make sure it is loaded.");
            return assembly.Location;
        }
    }
}