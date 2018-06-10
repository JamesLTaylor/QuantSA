using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrepareRelease
{
    /// <summary>
    /// This class generates files that allow the custom installer to know which files to include.
    /// </summary>
    internal class InstallerGenerator
    {
        private readonly List<string[]> fileList;

        private readonly string installFileInfoPath;
        private readonly string installFilesPartialPath;
        private readonly string resourcesPath;
        private readonly string rootpath;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerGenerator"/> class.
        /// </summary>
        /// <param name="rootpath">The rootpath for the QuantSA source.</param>
        /// <param name="installFilesPartialPath">The install files partial path.</param>
        /// <param name="tempOutputPath">The temporary output path.</param>
        public InstallerGenerator(string rootpath, string installFilesPartialPath)
        {
            this.rootpath = rootpath;
            this.installFilesPartialPath = installFilesPartialPath;
            installFileInfoPath =
                Path.Combine(rootpath, @"QuantSAInstaller\QuantSAInstaller\Resources\InstallFileInfo.csv");
            resourcesPath = Path.Combine(rootpath, @"QuantSAInstaller\QuantSAInstaller\Properties\Resources.resx");
            fileList = new List<string[]>();
        }

        public void Generate()
        {
            AddFile("QuantSA.xll", @"");
            AddFile("ExcelDna.IntelliSense.xll", @"");
            AddFile("QuantSA Help.url", @"");
            AddFile("QuantSA.dna", @"");
            AddFile("zipped_dlls.zip", @"");
            //AddFiles(@"", ".dll");
            AddFiles(@"ExcelExamples", ".xlsx");
            AddFiles(@"Scripts", ".cs");
            WriteInstallFileInfo();
            AddElementsToResourceFile();
        }

        /// <summary>
        /// Writes a file with lines: resource_name, relative_path_to_file, location_and_filename_to_install                
        /// </summary>
        private void WriteInstallFileInfo()
        {
            File.WriteAllLines(installFileInfoPath,
                fileList.Select(strArr => strArr[0] + "," + strArr[1] + "," + strArr[2]).ToArray());
        }


        /// <summary>
        /// Adds the XML elements to Resources.resx in the installer for each line in <see cref="fileList"/>
        /// </summary>
        /// <remarks>
        /// The XML element look like:
        /// 
        /// <![CDATA[<data name="CDS" type="System.Resources.ResXFileRef, System.Windows.Forms">]]>
        /// <![CDATA[<value>..\Resources\CDS.xlsx;System.Byte[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>]]>
        /// <![CDATA[</data>]]>
        /// 
        /// </remarks>
        /// <exception cref="Exception"></exception>
        private void AddElementsToResourceFile()
        {
            var originalLines = File.ReadAllLines(resourcesPath);
            var combinedLines = new List<string>();
            var i = 0;
            while (!originalLines[i].Contains("<!--AUTO-->") && i < originalLines.Length)
            {
                combinedLines.Add(originalLines[i]);
                i++;
            }

            if (i == originalLines.Length)
                throw new Exception(resourcesPath +
                                    " does not contain a line '<!--AUTO-->' which is required.  Revert this file from git and try again.");

            combinedLines.Add("<!--AUTO-->");
            combinedLines.Add(
                "<!--  Start of generated version.  See PrepareRelease.sln.  If the resources are edited in Visual Studio this comment will be removed and will be need to be reverted from git. -->");

            foreach (var line in fileList)
            {
                combinedLines.Add("  <data name=\"" + line[0] +
                                  "\" type=\"System.Resources.ResXFileRef, System.Windows.Forms\">");
                combinedLines.Add("    <value>" + line[1] + "\\" + line[2] +
                                  ";System.Byte[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>");
                combinedLines.Add("  </data>");
            }

            combinedLines.Add("</root>");
            File.WriteAllLines(resourcesPath, combinedLines.ToArray());
        }


        private void AddFiles(string subDir, string extension)
        {
            // Iterate over files in folder
            var directory = new DirectoryInfo(Path.Combine(rootpath, installFilesPartialPath, subDir));
            var files = directory.GetFiles();
            foreach (var fileInfo in files.Where(
                fileinfo => fileinfo.Extension.Equals(extension) &&
                            !fileinfo.Attributes.HasFlag(FileAttributes.Hidden)))
                AddFile(fileInfo.Name, subDir);
        }

        private void AddFile(string fileName, string subDir)
        {
            var resourceName = RemoveIllegalChars(fileName);
            var fileRelPath = Path.Combine(@"..\..\..\", installFilesPartialPath);
            var targetPath = Path.Combine(subDir, fileName);
            fileList.Add(new[] {resourceName, fileRelPath, targetPath});
        }

        private string RemoveIllegalChars(string stringIn)
        {
            var stringOut = stringIn.Replace('.', '_');
            stringOut = stringOut.Replace(' ', '_');
            return stringOut;
        }
    }
}