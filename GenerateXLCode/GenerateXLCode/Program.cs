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
                string classname = entry.Key + "Generated";
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
                List<string> defaultValues = new List<string>();
                foreach (ParameterInfo paramInfo in entry.Value.GetParameters())
                {
                    argTypes.Add(paramInfo.ParameterType);
                    argNames.Add(paramInfo.Name);
                    QuantSAExcelArgumentAttribute argAttrib = paramInfo.GetCustomAttribute<QuantSAExcelArgumentAttribute>();
                    if (argAttrib==null)
                        defaultValues.Add(null);
                    else
                        defaultValues.Add(argAttrib.Default);
                }
                string generatedMethod = GetGeneratedMethodCode(categoryName, xlName, fName,
                    argNames, argTypes, defaultValues, returnType);

                if (!categoriesAndGeneratedMethods.ContainsKey(categoryName)){
                    categoriesAndGeneratedMethods[categoryName] = new List<string>();
                }
                categoriesAndGeneratedMethods[categoryName].Add(generatedMethod);
            }
        }

        /// <summary>
        /// Gets the generated method string for a function call.
        /// </summary>
        /// <param name="categoryName">Name of the category in which the original function is found.</param>
        /// <param name="xlName">The exposed Excel name.</param>
        /// <param name="fName">The name of the public static C# method.</param>
        /// <param name="argNames">The argument names.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns></returns>
        private static string GetGeneratedMethodCode(string categoryName, string xlName, string fName, 
            List<string> argNames, List<Type> argTypes, List<string> defaultValues, Type returnType)
        {
            string returnTypeString;
            int dim = getTypeDimension(returnType);
            switch (dim) {
                case 0: returnTypeString = "object"; break;
                case 1: returnTypeString = "object[,]"; break;
                case 2: returnTypeString = "object[,]"; break;
                default: throw new ArgumentException("Dimension of return type must be scalar, array or 2d array.");                   
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Spaces(8) + "[QuantSAExcelFunction(Name = \"" + xlName + "\", IsGeneratedVersion = true)]");
            sb.Append(Spaces(8) + "public static " + returnTypeString + " _" + fName + "(");
            if (!ExcelUtilities.IsPrimitiveOutput(returnType))
                sb.AppendLine("string objectName,");            
                
            for (int i = 0; i < argNames.Count; i++) {
                if (ExcelUtilities.IsPrimitiveOutput(returnType) && i==0)
                    sb.Append("object[,] " + argNames[i]);
                else
                    sb.Append(Spaces(28) + "object[,] " + argNames[i]);
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
                sb.AppendLine(GetConverterString(argTypes[i], argNames[i], defaultValues[i]));
                if (i > 0) argCallList.Append(", ");
                argCallList.Append("_" + argNames[i]);
            }
            sb.AppendLine(Spaces(16) + returnType.Name + " _result = " + categoryName + "." + fName + "(" + argCallList.ToString() + ");");

            if (ExcelUtilities.IsPrimitiveOutput(returnType))
                sb.AppendLine(Spaces(16) + "return XU.ConvertToObjects(_result);");
            else
                sb.AppendLine(Spaces(16) + "return XU.AddObject(objectName, _result);");
            sb.AppendLine(Spaces(12) + "}");
            sb.AppendLine(Spaces(12) + "catch (Exception e)");
            sb.AppendLine(Spaces(12) + "{");
            if (dim==0)
                sb.AppendLine(Spaces(16) + "return XU.Error0D(e);");
            else
                sb.AppendLine(Spaces(16) + "return XU.Error2D(e);");
            sb.AppendLine(Spaces(12) + "}");
            sb.AppendLine(Spaces(8) + "}");
            return sb.ToString();
        }

        /// <summary>
        /// Create a string with n spaces.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private static string Spaces(int n)
        {
            return new string(' ', n);
        }


        /// <summary>
        /// Gets the code that converts object[,] to the required type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <returns></returns>
        private static string GetConverterString(Type type, string argName, string defaultValue)
        {
            string nD = "0D";
            if (type.Name.Contains(','))
                nD = "2D";
            else if (type.Name.Contains('['))
                nD = "1D";
            string name = type.IsArray ? type.GetElementType().Name : type.Name;
            if (ExcelUtilities.InputTypeHasConversion(type))
            {
                if (defaultValue== null)
                    return "XU.Get" + name + nD + "(" + argName + ", \"" + argName + "\");";
                else
                    return "XU.Get" + name + nD + "(" + argName + ", \"" + argName + "\", " + defaultValue + ");";
            }
            else
                return "XU.GetObject" + nD + "<" + name + ">(" + argName + ", \"" + argName + "\");";
        }

        /// <summary>
        /// Gets the dimension of the type, based on "Type" = 0, "Type[]" = 1 or "Type[,]" = 2
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static int getTypeDimension(Type type)
        {
            int dim = 0;
            if (type.Name.Contains(','))
                dim = 2;
            else if (type.Name.Contains('['))
                dim = 1;
            return dim;
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
