using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerateXLCode
{
    /// <summary>
    /// Generate Excel functions that take data types understood by ExcelDNA
    /// </summary>
    class Program
    {
        private static Dictionary<string, List<string>> categoriesAndGeneratedMethods = new Dictionary<string, List<string>>();

        static void Main(string[] args)
        {
            string filename = @"C:\Dev\QuantSA\QuantSA\Excel\bin\Debug\QuantSA.Excel.dll";
            string outputPath = @"C:\Dev\QuantSA\QuantSA\Excel\Generated\";
            Dictionary<string, MethodInfo> functions = GetFuncsWithGenVersion(filename);            
            GenerateMethodCode(functions);
            WriteFiles(outputPath);
        }

        private static void WriteFiles(string outputPath)
        {
            foreach (KeyValuePair<string, List<string>> entry in categoriesAndGeneratedMethods)
            {
                string classname = entry.Key + "Generated2";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using XU = QuantSA.Excel.ExcelUtilities;");
                sb.AppendLine("using QuantSA.General;");
                sb.AppendLine("using QuantSA.Valuation;");
                sb.AppendLine("");
                sb.AppendLine("namespace QuantSA.Excel");
                sb.AppendLine("{");
                sb.AppendLine(Spaces(4) + "public class " + classname);
                sb.AppendLine(Spaces(4) + "{");
                foreach (string methodBody in entry.Value)
                {
                    sb.Append(Environment.NewLine + methodBody + Environment.NewLine);
                }
                sb.AppendLine(Spaces(4) + "}");
                sb.AppendLine("}");
                File.WriteAllText(outputPath + classname + ".cs", sb.ToString());
            }

        }

        /// <summary>
        /// Generates the code for all the methods that have attribute value 
        /// QuantSAExcelFunctionAttribute.HasGeneratedVersion=true
        /// </summary>
        /// <param name="functions">The selected functions.</param>
        private static void GenerateMethodCode(Dictionary<string, MethodInfo> functions)
        {
            foreach (KeyValuePair<string, MethodInfo> entry in functions)
            {
                QuantSAExcelFunctionAttribute quantsaAttribute = entry.Value.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                string categoryName = entry.Value.DeclaringType.Name;
                string xlName = quantsaAttribute.Name;
                string fName = entry.Value.Name;
                Type returnType = entry.Value.ReturnType;
                List<Type> argTypes = new List<Type>();
                List<string> argNames = new List<string>();
                foreach (ParameterInfo paramInfo in entry.Value.GetParameters())
                {
                    argTypes.Add(paramInfo.ParameterType);
                    argNames.Add(paramInfo.Name);
                }
                string generatedMethod = GetMethodWithObjectReturn(categoryName, xlName, fName,
                    argNames, argTypes, returnType);

                if (!categoriesAndGeneratedMethods.ContainsKey(categoryName)){
                    categoriesAndGeneratedMethods[categoryName] = new List<string>();
                }
                categoriesAndGeneratedMethods[categoryName].Add(generatedMethod);
            }
        }

        /// <summary>
        /// Gets the generated method string for a function call that returns an object.
        /// </summary>
        /// <param name="categoryName">Name of the category in which the original function is found.</param>
        /// <param name="xlName">The exposed Excel name.</param>
        /// <param name="fName">The name of the public static C# method.</param>
        /// <param name="argNames">The argument names.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns></returns>
        private static string GetMethodWithObjectReturn(string categoryName, string xlName, string fName, 
            List<string> argNames, List<Type> argTypes, Type returnType)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Spaces(8) + "[QuantSAExcelFunction(Name = \"" + xlName + "\", IsGeneratedVersion = true)]");
            sb.AppendLine(Spaces(8) + "public static object _" + fName + "(string objectName,");
            for (int i = 0; i < argNames.Count; i++) {
                sb.Append(Spaces(28)+"object[,] " + argNames[i]);
                if (i == (argNames.Count - 1))
                    sb.AppendLine(")");
                else
                    sb.AppendLine(",");
            }
            sb.AppendLine(Spaces(8) + "{");
            sb.AppendLine(Spaces(12) + "try");
            sb.AppendLine(Spaces(12) + "{");
            StringBuilder argCallList = new StringBuilder();
            for (int i = 0; i < argNames.Count; i++)
            {
                sb.Append(Spaces(16) + argTypes[i].Name + " _" + argNames[i] + " = ");
                if (ExcelUtilities.InputTypeHasConversion(argTypes[i]))
                {
                    sb.AppendLine(GetConverterString(argTypes[i], argNames[i]));
                    if (i > 0) argCallList.Append(",");
                    argCallList.Append("_" + argNames[i]);
                }
            }
            sb.AppendLine(Spaces(16) + returnType.Name + " result = " + categoryName + "." + fName + "(" + argCallList.ToString() + ");");
            sb.AppendLine(Spaces(16) + "return XU.AddObject(objectName, result);");
            sb.AppendLine(Spaces(12) + "}");
            sb.AppendLine(Spaces(12) + "catch (Exception e)");
            sb.AppendLine(Spaces(12) + "{");
            sb.AppendLine(Spaces(16) + "return XU.Error0D(e);");
            sb.AppendLine(Spaces(12) + "}");
            sb.AppendLine(Spaces(8) + "}");
            return sb.ToString();
        }

        private static string Spaces(int n)
        {
            return new string(' ', n);
        }


        /// <summary>
        /// Gets the converter string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <returns></returns>
        private static string GetConverterString(Type type, string argName)
        {
            string nD = "0D";
            if (type.Name.Contains(','))
                nD = "2D";
            else if (type.Name.Contains('['))
                nD = "1D";
            string name = type.IsArray ? type.GetElementType().Name : type.Name;
            return "XU.Get" + name + nD + "(" +argName + ", \"" + argName + "\");";
        }


        /// <summary>
        /// Use reflection on Excel.dll to find all the members that have 
        /// the <see cref="QuantSAExcelFunctionAttribute"/> attribute.
        /// </summary>
        /// <remarks>
        /// This method is very simular to <see cref="GetFuncsWithGenVersion"/> in the AddIn</remarks>
        /// <returns></returns>
        public static Dictionary<string, MethodInfo> GetFuncsWithGenVersion(string filename)
        {
            Dictionary<string, MethodInfo> quantSAFunctions = new Dictionary<string, MethodInfo>();
            Assembly excelAssemby = Assembly.LoadFile(filename);
            Type[] types = null;
            try
            {
                types = excelAssemby.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
            }
            foreach (Type type in types)
            {
                if (!type.IsPublic)
                {
                    continue;
                }

                MemberInfo[] members = type.GetMembers();
                foreach (MemberInfo member in members)
                {
                    QuantSAExcelFunctionAttribute attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                    if (attribute != null)
                    {
                        if (attribute.HasGeneratedVersion) // if there is generated version then that will be used to constuct the delgate and this one will be used to get the help.
                            quantSAFunctions[attribute.Name] = (MethodInfo)member;
                    }
                }
            }
            return quantSAFunctions;
        }
    }
}
