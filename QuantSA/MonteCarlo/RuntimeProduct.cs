using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
    public class RuntimeProduct
    {
        public static Product CreateFromScript(String filename)
        {
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.Add("MonteCarlo.dll");
            parameters.ReferencedAssemblies.Add("General.dll");

            CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, new string[] { filename });
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
            if (results.CompiledAssembly.DefinedTypes.Count()>1)
            {
                throw new Exception("Assembly must only define one type : A Class that extends MonteCarlo.Product.");
            }
            string typeName = results.CompiledAssembly.DefinedTypes.First().Name;
            Type productType = results.CompiledAssembly.GetType(typeName);
            if (!typeof(Product).IsAssignableFrom(productType))
            {
                throw new Exception("The defined type must derive from MonteCarlo.Product");
            }
            
            return (Product)Activator.CreateInstance(productType);
        }
    }
}
