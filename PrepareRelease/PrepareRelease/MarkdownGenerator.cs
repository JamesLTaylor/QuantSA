using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExcelDna.Integration;
using QuantSA.Excel.Addin.Functions;
using QuantSA.Excel.Shared;

namespace PrepareRelease
{
    /// <summary>
    /// Stores the generated files in a hierarchy of Category -> name -> contents.
    /// </summary>
    internal class ContentCollection
    {
        private readonly Dictionary<string, Dictionary<string, FileContents>> _structure;

        public ContentCollection()
        {
            _structure = new Dictionary<string, Dictionary<string, FileContents>>();
        }


        public void AddFile(FileContents fileContents)
        {
            var cat = fileContents.Category;
            Dictionary<string, FileContents> subMenu;
            if (_structure.ContainsKey(cat))
            {
                subMenu = _structure[cat];
            }
            else
            {
                subMenu = new Dictionary<string, FileContents>();
                _structure[cat] = subMenu;
            }

            subMenu[fileContents.Name] = fileContents;
        }

        public List<string> GetCategories()
        {
            var keys = _structure.Keys.ToList();
            keys.Sort();
            return keys;
        }

        public List<string> GetNames(string category)
        {
            var keys = _structure[category].Keys.ToList();
            keys.Sort();
            return keys;
        }

        public FileContents Get(string category, string name)
        {
            Dictionary<string, FileContents> subMenu;
            if (!_structure.ContainsKey(category))
            {
                subMenu = new Dictionary<string, FileContents>();
                _structure[category] = subMenu;
            }

            subMenu = _structure[category];
            if (!subMenu.ContainsKey(name)) subMenu[name] = new FileContents();
            return _structure[category][name];
        }
    }

    /// <summary>
    /// Stores the information for a generated Markdown file.
    /// </summary>
    internal class FileContents
    {
        private string _description;
        public List<string> ArgDescriptions;
        public List<string> ArgNames;
        public string Category;
        public string ExampleSheet;
        public string Name;
        public string Summary;

        public FileContents()
        {
            ArgNames = new List<string>();
            ArgDescriptions = new List<string>();
        }

        public string Description
        {
            get => _description;

            set
            {
                _description = value;
                var parts = _description.Split('.');
                Summary = parts[0] + ".";
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
        private readonly string[] _dllsWithExposedFunctions;
        private readonly List<string> _documentedTypes;
        private readonly string _helpUrl;
        private readonly string _outputPath;
        private readonly string _tempOutputPath;
        private ContentCollection _contentCollection;
        private List<string> _errorInfo;

        public MarkdownGenerator(string[] dllsWithExposedFunctions, string outputPath, string helpURL,
            string tempOutputPath)
        {
            _dllsWithExposedFunctions = dllsWithExposedFunctions;
            _outputPath = outputPath;
            _helpUrl = helpURL;
            _tempOutputPath = tempOutputPath;
            _documentedTypes = new List<string>();
        }

        public int Generate()
        {
            _contentCollection = new ContentCollection();
            _errorInfo = new List<string>();
            UpdateContentCollection();
            UpdateHelpYMLAndWriteMD();
            File.WriteAllLines(Path.Combine(_tempOutputPath, MD_ERROR_OUTPUT_FILE), _errorInfo.ToArray());
            return _errorInfo.Count();
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
            if (_contentCollection == null)
                throw new Exception("Call Generate() before calling AreAllExampleSheetsValid().");
            foreach (var category in _contentCollection.GetCategories())
            foreach (var name in _contentCollection.GetNames(category))
            {
                var contents = _contentCollection.Get(category, name);
                var listedExample = contents.ExampleSheet;
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

            File.WriteAllLines(Path.Combine(_tempOutputPath, SS_ERROR_OUTPUT_FILE), errorList.ToArray());
            return errorList.Count();
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateHelpYMLAndWriteMD()
        {
            var sidebarFile = _outputPath + @"_data\sidebars\excel_sidebar.yml";

            string line;
            var file = new StreamReader(sidebarFile);
            var newFileLines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                newFileLines.Add(line);
                if (line.Contains("title:"))
                {
                    var idx = line.IndexOf("title:");
                    _documentedTypes.Add(line.Substring(idx + 6).Trim());
                }

                if (line.Contains("AUTO GENERATED BEYOND THIS POINT")) break;
            }

            file.Close();

            foreach (var category in _contentCollection.GetCategories())
            {
                newFileLines.Add(@"  - title: " + category);
                newFileLines.Add(@"    output: web, pdf");
                newFileLines.Add(@"    folderitems:");
                foreach (var name in _contentCollection.GetNames(category))
                {
                    newFileLines.Add(@"    - title: " + name);
                    newFileLines.Add(@"      url: /" + name + ".html");
                    newFileLines.Add(@"      output: web, pdf");
                    UpdateOrCreateMD(_outputPath, _contentCollection.Get(category, name));
                }
            }

            File.WriteAllLines(sidebarFile, newFileLines, Encoding.UTF8);
        }

        private void UpdateOrCreateMD(string outputPath, FileContents fileContents)
        {
            var markdownFilePath = outputPath + @"\pages\excel\" + fileContents.Name + ".md";
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
                        newFileLines.Add(@"summary: " + fc.Summary);
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
            newFileLines.Add(@"title: " + fc.Name);
            newFileLines.Add(@"keywords:");
            newFileLines.Add(@"last_updated: " + now.ToString("MMMM dd, yyyy"));
            newFileLines.Add(@"tags:");
            newFileLines.Add(@"summary: " + fc.Summary);
            newFileLines.Add(@"sidebar: excel_sidebar");
            newFileLines.Add(@"permalink: " + fc.Name + ".html");
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
            newFileLines.Add(@"    " + fc.ExampleSheet);
            newFileLines.Add(@"");
            newFileLines.Add(@"## Arguments");
            newFileLines.Add(@"");
            for (var i = 0; i < fc.ArgNames.Count; i++)
                newFileLines.Add(@"* **" + fc.ArgNames[i] + "** " + AddLinks(fc.ArgDescriptions[i]));
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
            foreach (var typeName in _documentedTypes)
                if (input.Contains(typeName))
                {
                    var idx = input.IndexOf(typeName);
                    if (idx >= 1 && input[idx - 1] == '(')
                        input = input.Substring(0, idx) + "[" + typeName + "](" + typeName + ".html)" +
                                input.Substring(idx + typeName.Length);
                }

            return input;
        }


        /// <summary>
        /// Turns the attributes on all functions exposed as excel functions in <see cref="_dllsWithExposedFunctions"/> into
        /// contents for help files.
        /// Check the local variable errorList for a list of consistency issues that should be fixed.
        /// </summary>
        private void UpdateContentCollection()
        {
            foreach (var dllName in _dllsWithExposedFunctions) UpdateContentCollectionSingleDll(dllName);
        }

        /// <summary>
        /// Updates the content collection for a single dll.
        /// </summary>
        /// <param name="dllName">Name of the DLL.</param>
        private void UpdateContentCollectionSingleDll(string dllName)
        {
            var assembly = Assembly.LoadFrom(dllName);
            var delegates = new List<Delegate>();
            var functionAttributes = new List<object>();
            var functionArgumentAttributes = new List<List<object>>();
            FunctionRegistration.GetDelegatesAndAttributes(assembly, "QSA", ref delegates, ref functionAttributes,
                ref functionArgumentAttributes);
            for (var i = 0; i < delegates.Count; i++)
            {
                var attribute = functionAttributes[i] as QuantSAExcelFunctionAttribute;
                var contents = new FileContents();
                // Check consistency of contents
                var nameParts = attribute.Name.Split('.');
                if (attribute.Description.Length < 5)
                    _errorInfo.Add(attribute.Category + "," + attribute.Name + ",,Does not have a description.");
                if (!nameParts[0].Equals("QSA"))
                    _errorInfo.Add(attribute.Category + "," + attribute.Name + ",,Name does not start with 'QSA'.");
                if (attribute.HelpTopic == null)
                    _errorInfo.Add(attribute.Category + "," + attribute.Name + ",,Does not have a help topic.");
                else if (!attribute.HelpTopic.Equals(_helpUrl + nameParts[1] + ".html"))
                    _errorInfo.Add(attribute.Category + "," + attribute.Name + ",,Help topic should be," + _helpUrl +
                                   nameParts[1] + ".html");
                if (attribute.ExampleSheet == null)
                {
                    _errorInfo.Add(attribute.Category + "," + attribute.Name + ",,Example sheet has not been set.");
                    contents.ExampleSheet = "Not available";
                }
                else
                {
                    contents.ExampleSheet = attribute.ExampleSheet;
                }

                // Add details
                contents.Name = attribute.Name.Split('.')[1];
                contents.Category = attribute.Category.Split('.')[1];
                contents.Description = attribute.Description;

                foreach (var argAttribObj in functionArgumentAttributes[i])
                {
                    var argAttrib = argAttribObj as ExcelArgumentAttribute;
                    contents.ArgNames.Add(argAttrib.Name);
                    // TODO: JT: Replace (type) with reference.

                    contents.ArgDescriptions.Add(argAttrib.Description);
                    if (argAttrib.Description.Length < 5)
                        _errorInfo.Add(attribute.Category + "," + attribute.Name + "," + argAttrib.Name +
                                       ",No argument description");
                }

                _contentCollection.AddFile(contents);
            }
        }
    }
}