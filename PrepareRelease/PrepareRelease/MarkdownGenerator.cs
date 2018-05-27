using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;

namespace PrepareRelease
{
    /// <summary>
    /// Stores the generated files in a hierarchy of Category -> name -> contents.
    /// </summary>
    internal class contentCollection
    {
        private readonly Dictionary<string, Dictionary<string, FileContents>> structure;

        public contentCollection()
        {
            structure = new Dictionary<string, Dictionary<string, FileContents>>();
        }


        public void AddFile(FileContents fileContents)
        {
            var cat = fileContents.category;
            Dictionary<string, FileContents> subMenu;
            if (structure.ContainsKey(cat))
            {
                subMenu = structure[cat];
            }
            else
            {
                subMenu = new Dictionary<string, FileContents>();
                structure[cat] = subMenu;
            }

            subMenu[fileContents.name] = fileContents;
        }

        public List<string> GetCategories()
        {
            var keys = structure.Keys.ToList();
            keys.Sort();
            return keys;
        }

        public List<string> GetNames(string category)
        {
            var keys = structure[category].Keys.ToList();
            keys.Sort();
            return keys;
        }

        public FileContents Get(string category, string name)
        {
            Dictionary<string, FileContents> subMenu;
            if (!structure.ContainsKey(category))
            {
                subMenu = new Dictionary<string, FileContents>();
                structure[category] = subMenu;
            }

            subMenu = structure[category];
            if (!subMenu.ContainsKey(name)) subMenu[name] = new FileContents();
            return structure[category][name];
        }
    }

    /// <summary>
    /// Stores the information for a generated Markdown file.
    /// </summary>
    internal class FileContents
    {
        public List<string> argDescriptions;
        public List<string> argNames;
        public string category;
        private string description;
        public string exampleSheet;
        public string name;
        public string summary;

        public FileContents()
        {
            argNames = new List<string>();
            argDescriptions = new List<string>();
        }

        public string Description
        {
            get { return description; }

            set
            {
                description = value;
                var parts = description.Split('.');
                summary = parts[0] + ".";
            }
        }
    }

    /// <summary>
    /// Generate markdown files from the Excel argument attributes.
    /// </summary>
    internal class MarkdownGenerator
    {
        public const string MD_ERROR_OUTPUT_FILE = "MarkdownGenerator.csv";
        public const string SS_ERROR_OUTPUT_FILE = "AreExampleSheetsValid.csv";
        private contentCollection contentCollection;
        private readonly string[] dllsWithExposedFunctions;
        private readonly List<string> documentedTypes;
        private List<string> errorInfo;
        private readonly Dictionary<string, int> generatedMethodArgCount;
        private readonly string helpURL;
        private readonly string outputPath;
        private readonly string tempOutputPath;

        public MarkdownGenerator(string[] dllsWithExposedFunctions, string outputPath, string helpURL,
            string tempOutputPath)
        {
            this.dllsWithExposedFunctions = dllsWithExposedFunctions;
            this.outputPath = outputPath;
            this.helpURL = helpURL;
            this.tempOutputPath = tempOutputPath;
            documentedTypes = new List<string>();
            generatedMethodArgCount = new Dictionary<string, int>();
        }

        public int Generate()
        {
            contentCollection = new contentCollection();
            errorInfo = new List<string>();
            UpdateContentCollection();
            UpdateHelpYMLAndWriteMD();
            File.WriteAllLines(Path.Combine(tempOutputPath, MD_ERROR_OUTPUT_FILE), errorInfo.ToArray());
            return errorInfo.Count();
        }


        /// <summary>
        /// Checks if all the example sheets listed in the markdown are.
        /// <para/>
        /// Only call after Generate has been called.
        /// </summary>
        /// <returns></returns>
        public int AreAllExampleSheetsValid(Dictionary<string, HashSet<string>> sheetsAndFuncs)
        {
            var errorList = new List<string>();
            if (contentCollection == null)
                throw new Exception("Call Generate() before calling AreAllExampleSheetsValid().");
            foreach (var category in contentCollection.GetCategories())
            foreach (var name in contentCollection.GetNames(category))
            {
                var contents = contentCollection.Get(category, name);
                var listedExample = contents.exampleSheet;
                if (sheetsAndFuncs.ContainsKey(listedExample))
                {
                    var funcs = sheetsAndFuncs[listedExample];
                    if (!funcs.Contains(name))
                        errorList.Add(name + ", Lists '" + listedExample + "' but that sheet does not call it.");
                }
                else
                {
                    errorList.Add(name + ", Lists '" + listedExample + "' but that sheet does not exist.");
                }
            }

            File.WriteAllLines(Path.Combine(tempOutputPath, SS_ERROR_OUTPUT_FILE), errorList.ToArray());
            return errorList.Count();
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateHelpYMLAndWriteMD()
        {
            var sidebarFile = outputPath + @"_data\sidebars\excel_sidebar.yml";

            string line;
            var file = new StreamReader(sidebarFile);
            var newFileLines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                newFileLines.Add(line);
                if (line.Contains("title:"))
                {
                    var idx = line.IndexOf("title:");
                    documentedTypes.Add(line.Substring(idx + 6).Trim());
                }

                if (line.Contains("AUTO GENERATED BEYOND THIS POINT")) break;
            }

            file.Close();

            foreach (var category in contentCollection.GetCategories())
            {
                newFileLines.Add(@"  - title: " + category);
                newFileLines.Add(@"    output: web, pdf");
                newFileLines.Add(@"    folderitems:");
                foreach (var name in contentCollection.GetNames(category))
                {
                    newFileLines.Add(@"    - title: " + name);
                    newFileLines.Add(@"      url: /" + name + ".html");
                    newFileLines.Add(@"      output: web, pdf");
                    UpdateOrCreateMD(outputPath, contentCollection.Get(category, name));
                }
            }

            File.WriteAllLines(sidebarFile, newFileLines, Encoding.UTF8);
        }

        private void UpdateOrCreateMD(string outputPath, FileContents fileContents)
        {
            var markdownFilePath = outputPath + @"\pages\excel\" + fileContents.name + ".md";
            if (File.Exists(markdownFilePath))
                UpdateExistingMD(markdownFilePath, fileContents);
            else
                CreateNewMD(markdownFilePath, fileContents);
        }

        /// <summary>
        /// Updates an existing mark down file with information that may have changed in 
        /// the function attributes.
        /// </summary>
        /// <param name="markdownFilePath"></param>
        /// <param name="fileContents"></param>
        private void UpdateExistingMD(string markdownFilePath, FileContents fc)
        {
            string line;
            var file = new StreamReader(markdownFilePath);
            var newFileLines = new List<string>();
            var sectionNumber = 0;
            // Until the first HUMAN EDIT END include all the lines except summary and description
            // After that ignore and regenerate all lines until HUMAN EDIT START is found again
            while ((line = file.ReadLine()) != null)
            {
                if (sectionNumber == 0)
                {
                    if (line.Length > 8 && line.StartsWith("summary"))
                    {
                        newFileLines.Add(@"summary: " + fc.summary);
                    }
                    else if (line.Length > 8 && line.StartsWith("## Description"))
                    {
                        file.ReadLine(); // step on an extra line
                        newFileLines.Add(@"## Description");
                        newFileLines.Add(fc.Description);
                    }
                    else
                    {
                        newFileLines.Add(line);
                    }

                    if (line.Contains("HUMAN EDIT END"))
                    {
                        sectionNumber = 1;
                        newFileLines.AddRange(MakeStandardLines(fc));
                    }
                }

                if (sectionNumber == 1) // spin until then next HUMAN EDIT START section is found.
                    if (line.Contains("HUMAN EDIT START"))
                        sectionNumber = 2;
                if (sectionNumber == 2) // Include everything in the last section
                    newFileLines.Add(line);
            }

            file.Close();
            File.WriteAllLines(markdownFilePath, newFileLines);
        }

        /// <summary>
        /// Writes a new markdown file.
        /// </summary>
        /// <param name="markdownFilePath">The markdown file path.</param>
        /// <param name="fc">The information about what should be in the file.</param>
        private void CreateNewMD(string markdownFilePath, FileContents fc)
        {
            var now = DateTime.Now;
            var newFileLines = new List<string>();
            newFileLines.Add(@"---");
            newFileLines.Add(@"title: " + fc.name);
            newFileLines.Add(@"keywords:");
            newFileLines.Add(@"last_updated: " + now.ToString("MMMM dd, yyyy"));
            newFileLines.Add(@"tags:");
            newFileLines.Add(@"summary: " + fc.summary);
            newFileLines.Add(@"sidebar: excel_sidebar");
            newFileLines.Add(@"permalink: " + fc.name + ".html");
            newFileLines.Add(@"folder: excel");
            newFileLines.Add(@"---");
            newFileLines.Add(@"");
            newFileLines.Add(@"## Description");
            newFileLines.Add(fc.Description);
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--HUMAN EDIT START-->");
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--## Details-->");
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--HUMAN EDIT END-->");
            newFileLines.AddRange(MakeStandardLines(fc));
            newFileLines.Add(@"<!--HUMAN EDIT START-->");
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--## Validation-->");
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--HUMAN EDIT END-->");
            newFileLines.Add(@"");
            File.WriteAllLines(markdownFilePath, newFileLines);
        }


        /// <summary>
        /// Makes the standard example sheet and argument description lines.
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        private List<string> MakeStandardLines(FileContents fc)
        {
            var newFileLines = new List<string>();
            newFileLines.Add(@"");
            newFileLines.Add(@"## Example Sheet");
            newFileLines.Add(@"");
            newFileLines.Add(@"    " + fc.exampleSheet);
            newFileLines.Add(@"");
            newFileLines.Add(@"## Arguments");
            newFileLines.Add(@"");
            for (var i = 0; i < fc.argNames.Count; i++)
                newFileLines.Add(@"* **" + fc.argNames[i] + "** " + AddLinks(fc.argDescriptions[i]));
            newFileLines.Add(@"");
            return newFileLines;
        }


        /// <summary>
        /// Should the input of this type include a link to the help about that type?  For example
        /// if the input type is <see cref="FloatingIndex"/> then it is useful to link to the
        /// page on FloatingIndex so that the user can see the permissible strings.
        /// </summary>
        /// <remarks>
        /// If you update the values in this method then you probably need to update the values in
        /// GenerateXLCode.TypeInformation.InputTypeHasConversion
        /// </remarks>        
        /// <param name="inputType">Type of the input.</param>
        /// <returns></returns>
        private bool InputTypeShouldHaveHelpLink(Type inputType)
        {
            //TODO: This method should be replaced with checking if the type is in documentedTypes.  That would mean one less thing to maintain.
            var type = inputType.IsArray ? inputType.GetElementType() : inputType;
            if (type == typeof(bool)) return true;
            if (type.Name == "Date") return true;
            if (type.Name == "Currency") return true;
            if (type.Name == "FloatingIndex") return true;
            if (type.Name == "Tenor") return true;
            if (type.Name == "Share") return true;
            if (type.Name == "ReferenceEntity") return true;
            if (type.Name == "CompoundingConvention") return true;
            if (type.Name == "DayCountConvention") return true;
            if (type.Name == "BusinessDayConvention") return true;
            if (type.Name == "Calendar") return true;
            return false;
        }

        /// <summary>
        /// Converts text like (Currency) to ([Currency](Currency.html))
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string AddLinks(string input)
        {
            foreach (var typeName in documentedTypes)
                if (input.Contains(typeName))
                {
                    var idx = input.IndexOf(typeName);
                    if (idx > 1 && input[idx - 1] == '(' && input[idx + typeName.Length] == ')')
                        input = input.Substring(0, idx) + "[" + typeName + "](" + typeName + ".html)" +
                                input.Substring(idx + typeName.Length);
                }

            return input;
        }


        /// <summary>
        /// Turns the attributes on all functions exposed as excel functions in <paramref name="filename" /> into
        /// contents for help files.
        /// Check the local variable errorList for a list of consistency issues that should be fixed.
        /// </summary>
        private void UpdateContentCollection()
        {
            foreach (var dllName in dllsWithExposedFunctions) UpdateContentCollection1dll(dllName);
            // Insert an extra argument description for those methods that have been generated with the first
            // argument being the name of the object to use in Excel.
            foreach (var category in contentCollection.GetCategories())
            foreach (var name in contentCollection.GetNames(category))
            {
                var contents = contentCollection.Get(category, name);
                if (generatedMethodArgCount.ContainsKey(name) &&
                    contents.argNames.Count() < generatedMethodArgCount[name])
                {
                    contents.argNames.Insert(0, "objectName");
                    contents.argDescriptions.Insert(0, "The name of the object to be created.");
                }
            }
        }

        /// <summary>
        /// Updates the content collection for a single dll.
        /// </summary>
        /// <param name="dllName">Name of the DLL.</param>
        private void UpdateContentCollection1dll(string dllName)
        {
            var DLL = Assembly.LoadFrom(dllName);
            foreach (var type in DLL.GetExportedTypes())
            foreach (var member in type.GetMembers())
            {
                var attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                if (attribute != null && attribute.IsGeneratedVersion)
                {
                    var method = (MethodInfo) member;
                    var name = attribute.Name.Split('.')[1];
                    var argCount = method.GetParameters().Count();
                    generatedMethodArgCount[name] = argCount;
                }

                if (attribute != null && !attribute.IsGeneratedVersion
                ) // We have found an excel exposed function and it is not the generated version.
                {
                    var contents = new FileContents();
                    // Check consistency of contents
                    var categoryParts = attribute.Category.Split('.');
                    var nameParts = attribute.Name.Split('.');
                    if (attribute.Description.Length < 5)
                        errorInfo.Add(type.Name + "," + attribute.Name + ",,Does not have a description.");
                    if (!nameParts[0].Equals("QSA"))
                        errorInfo.Add(type.Name + "," + attribute.Name + ",,Name does not start with 'QSA'.");
                    if (!("XL" + categoryParts[1]).Equals(type.Name))
                        errorInfo.Add(type.Name + "," + attribute.Name + ",,Category does not match file.");
                    if (attribute.HelpTopic == null)
                        errorInfo.Add(type.Name + "," + attribute.Name + ",,Does not have a help topic.");
                    else if (!attribute.HelpTopic.Equals(helpURL + nameParts[1] + ".html"))
                        errorInfo.Add(type.Name + "," + attribute.Name + ",,Help topic should be," + helpURL +
                                      nameParts[1] + ".html");
                    if (attribute.ExampleSheet == null)
                    {
                        errorInfo.Add(type.Name + "," + attribute.Name + ",,Example sheet has not been set.");
                        contents.exampleSheet = "Not available";
                    }
                    else
                    {
                        contents.exampleSheet = attribute.ExampleSheet;
                    }

                    // Add details
                    contents.name = attribute.Name.Split('.')[1];
                    contents.category = attribute.Category.Split('.')[1];
                    contents.Description = attribute.Description;

                    var method = (MethodInfo) member;
                    foreach (var param in method.GetParameters())
                    {
                        var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
                        if (argAttrib != null)
                        {
                            contents.argNames.Add(param.Name);
                            if (InputTypeShouldHaveHelpLink(param.ParameterType))
                            {
                                var name = param.ParameterType.IsArray
                                    ? param.ParameterType.GetElementType().Name
                                    : param.ParameterType.Name;
                                argAttrib.Description += "(" + name + ")";
                            }

                            contents.argDescriptions.Add(argAttrib.Description);

                            if (argAttrib.Description.Length < 5)
                                errorInfo.Add(type.Name + "," + attribute.Name + "," + param.Name +
                                              ",No argument description");
                        }
                        else
                        {
                            errorInfo.Add(type.Name + "," + attribute.Name + "," + param.Name +
                                          ",Argument does not have ExcelArgumentAttribute");
                        }
                    }

                    contentCollection.AddFile(contents);
                }
            }
        }
    }
}