using ExcelDna.Integration;
using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerateDocs
{
    class FolderStructure
    {
        Dictionary<string, Dictionary<string, FileContents>> structure;

        public FolderStructure()
        {
            structure = new Dictionary<string, Dictionary<string, FileContents>>();
        }
        public void AddFile(FileContents fileContents)
        {
            string cat = fileContents.category;
            Dictionary<string, FileContents> subMenu;
            if (structure.ContainsKey(cat))
                subMenu = structure[cat];
            else
            {
                subMenu = new Dictionary<string, FileContents>();
                structure[cat] = subMenu;
            }
            subMenu[fileContents.name] = fileContents;
        }
        public List<string> GetCategories()
        {
            List<string> keys = structure.Keys.ToList();
            keys.Sort();
            return keys;
        }
        public List<string> GetNames(string category)
        {
            List<string> keys = structure[category].Keys.ToList();
            keys.Sort();
            return keys;
        }
        public FileContents Get(string category, string name)
        {
            return structure[category][name];
        }
    }

    class FileContents
    {
        public string name;
        public string category;
        public string summary;
        public string exampleSheet;
        private string description;
        public List<string> argNames;
        public List<string> argDescriptions;

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
                string[] parts = description.Split('.');
                summary = parts[0] + ".";
            }
        }

        public FileContents()
        {
            argNames = new List<string>();
            argDescriptions = new List<string>();
        }
    };
        
    class Program
    {
        private static List<string> documentedTypes;
        static void Main(string[] args)
        {
            documentedTypes = new List<string>();
            string filename = @"C:\Dev\QuantSA\QuantSA\Excel\bin\Debug\QuantSA.Excel.dll";
            string outputPath = @"C:\Dev\jamesltaylor.github.io\";            
            string helpPath = "http://cogn.co.za/QuantSA/";
            FolderStructure folderStructure = GetFolderStructure(filename, helpPath);
            UpdateHelpYML(outputPath, folderStructure);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="folderStructure"></param>
        private static void UpdateHelpYML(string outputPath, FolderStructure folderStructure)
        {
            string sidebarFile = outputPath + @"_data\sidebars\excel_sidebar.yml";

            string line;
            StreamReader file = new StreamReader(sidebarFile);
            List<string> newFileLines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                newFileLines.Add(line);
                if (line.Contains("title:"))
                {
                    int idx = line.IndexOf("title:");
                    documentedTypes.Add(line.Substring(idx + 6).Trim());
                }
                if (line.Contains("AUTO GENERATED BEYOND THIS POINT")) break;
            }
            file.Close();

            foreach (string category in folderStructure.GetCategories())
            {
                newFileLines.Add(@"  - title: " + category);
                newFileLines.Add(@"    output: web, pdf");
                newFileLines.Add(@"    folderitems:");
                foreach (string name in folderStructure.GetNames(category))
                {
                    newFileLines.Add(@"    - title: " + name);
                    newFileLines.Add(@"      url: /" + name + ".html");
                    newFileLines.Add(@"      output: web, pdf");
                    UpdateHelpMD(outputPath, folderStructure.Get(category, name));
                }
            }
            File.WriteAllLines(sidebarFile, newFileLines, Encoding.UTF8);
        }

        private static void UpdateHelpMD(string outputPath, FileContents fileContents)
        {
            string markdownFilePath = outputPath + @"\pages\excel\" + fileContents.name + ".md";
            if (File.Exists(markdownFilePath))
            {
                UpdateEixstingMD(markdownFilePath, fileContents);
            }
            else
            {
                WriteNewMD(markdownFilePath, fileContents);
            }
        }

        /// <summary>
        /// Updates an existing mark down file with information that may have changed in the function attributes.
        /// </summary>
        /// <param name="markdownFilePath"></param>
        /// <param name="fileContents"></param>
        private static void UpdateEixstingMD(string markdownFilePath, FileContents fc)
        {
            string line;
            StreamReader file = new StreamReader(markdownFilePath);
            List<string> newFileLines = new List<string>();
            int sectionNumber = 0;
            // Until the first HUMAN EDIT END include all the lines except summary and description
            // After that ignore and regenerate all lines until HUMAN EDIT START is found again
            while ((line = file.ReadLine()) != null)
            {
                if (sectionNumber == 0)
                {
                    if (line.Length > 8 && line.StartsWith("summary"))
                        newFileLines.Add(@"summary: " + fc.summary);
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
                        newFileLines.AddRange(GetStandardLines(fc));
                    }
                }
                if (sectionNumber == 1) // spin until then next HUMAN EDIT START section is found.
                {
                    if (line.Contains("HUMAN EDIT START"))
                    {
                        sectionNumber = 2;                        
                    }
                }
                if (sectionNumber == 2) // Include everything in the last section
                {
                    newFileLines.Add(line);
                }
            }
            file.Close();
            File.WriteAllLines(markdownFilePath, newFileLines);
        }

        private static void WriteNewMD(string markdownFilePath, FileContents fc)
        {
            DateTime now = DateTime.Now;
            List<string> newFileLines = new List<string>();
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
            newFileLines.AddRange(GetStandardLines(fc));
            newFileLines.Add(@"<!--HUMAN EDIT START-->");
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--## Validation-->");
            newFileLines.Add(@"");
            newFileLines.Add(@"<!--HUMAN EDIT END-->");
            newFileLines.Add(@"");
            File.WriteAllLines(markdownFilePath, newFileLines);
        }


        /// <summary>
        /// Get the standard example sheet and argument description lines.
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        private static List<string> GetStandardLines(FileContents fc)
        {
            List<string> newFileLines = new List<string>();
            newFileLines.Add(@"");
            newFileLines.Add(@"## Example Sheet");
            newFileLines.Add(@"");
            newFileLines.Add(@"    " + fc.exampleSheet);            
            newFileLines.Add(@"");
            newFileLines.Add(@"## Arguments");
            newFileLines.Add(@"");
            for (int i = 0; i < fc.argNames.Count; i++)
            {
                newFileLines.Add(@"* **" + fc.argNames[i] + "** " + AddLinks(fc.argDescriptions[i]));
            }
            newFileLines.Add(@"");
            return newFileLines;
        }

        /// <summary>
        /// Converts text like (Currency) to ([Currency](Currency.html))
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string AddLinks(string input)
        {            
            foreach (string typeName in documentedTypes)
            {
                if (input.Contains(typeName))
                {
                    int idx = input.IndexOf(typeName);
                    if (input[idx - 1]=='(' && input[idx + typeName.Length]==')')
                    {
                        input = input.Substring(0, idx) + "[" + typeName + "](" + typeName + ".html)" + input.Substring(idx + typeName.Length);
                    }

                }
            }
            return input;
        }


        /// <summary>
        /// Turns the attrubutes on all function exposed as excel function in <paramref name="filename"/> into 
        /// contents for help files.
        /// 
        /// Check the local variable errorList for a list consistency issues that should be fixed.
        /// </summary>
        /// <param name="filename">The name of the dll that exposes Excel functions.</param>
        /// <param name="helpPath">Used to check that all help references are correct.</param>
        /// <returns></returns>
        private static FolderStructure GetFolderStructure(string filename, string helpPath)
        {
            FolderStructure folderStructure = new FolderStructure();
            Assembly DLL = Assembly.LoadFile(filename);
            List<string> errorList = new List<string>();
            foreach (Type type in DLL.GetExportedTypes())
            {
                foreach (MemberInfo member in type.GetMembers())
                {
                    QuantSAExcelFunctionAttribute attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                    if (attribute != null) // We have found an excel exposed function.
                    {
                        FileContents contents = new FileContents();
                        // Check consistency of contents
                        string[] categoryParts = attribute.Category.Split('.');
                        string[] nameParts = attribute.Name.Split('.');
                        if (attribute.Description.Length < 5) errorList.Add(type.Name + "," + attribute.Name + ",,Does not have a description.");
                        if (!nameParts[0].Equals("QSA")) errorList.Add(type.Name + "," + attribute.Name + ",,Name does not start with 'QSA'.");
                        if (!("XL" + categoryParts[1]).Equals(type.Name)) errorList.Add(type.Name + "," + attribute.Name + ",,Category does not match file.");
                        if (attribute.HelpTopic == null)
                        {
                            errorList.Add(type.Name + "," + attribute.Name + ",,Does not have a help topic.");
                        }
                        else if (!attribute.HelpTopic.Equals(helpPath + nameParts[1] + ".html"))
                        {
                            errorList.Add(type.Name + "," + attribute.Name + ",,Help topic should be," + helpPath + nameParts[1] + ".html");
                        }
                        if (attribute.ExampleSheet== null)
                        {
                            errorList.Add(type.Name + "," + attribute.Name + ",,Example sheet has not been set.");
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

                        MethodInfo method = (MethodInfo)member;
                        foreach (ParameterInfo param in method.GetParameters())
                        {
                            var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
                            if (argAttrib != null)
                            {
                                contents.argNames.Add(param.Name);
                                contents.argDescriptions.Add(argAttrib.Description);
                                if (argAttrib.Description.Length < 5)
                                    errorList.Add(type.Name + "," + attribute.Name + "," + param.Name + ",No argument description");
                            }
                            else
                            {
                                errorList.Add(type.Name + "," + attribute.Name + "," + param.Name + ",Argument does not have ExcelArgumentAttribute");
                            }
                        }
                        folderStructure.AddFile(contents);
                    }
                }
            }
            return folderStructure;
        }//GetFolderStructure

    }// Program
}
