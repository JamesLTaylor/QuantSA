using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QuantSAInstaller
{
    class Installer
    {
        string installPath;


        public void Start(string installPath, IProgress<string> progressOutput, IProgress<string> progressStep, CancellationToken cancellationToken)
        {
            try {
                //this.installPath = @"c:\temp";
                this.installPath = installPath;
                string folder = Path.Combine(this.installPath, "QuantSA");
                string installFileInfoRaw = Properties.Resources.InstallFileInfo;
                string[] lines = installFileInfoRaw.Split('\n');
                string[,] installFileInfo = new string[lines.Length, 3];

                progressOutput.Report("Started");

                if (Directory.Exists(folder))
                {
                    progressStep.Report("Removing old Files");
                    Directory.Delete(folder, true);
                }
                //Create folder
                progressStep.Report("Creating Folder");
                Directory.CreateDirectory(folder);
                progressOutput.Report(folder);

                //Install Files
                progressStep.Report("Installing Files");
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] cols = lines[i].Trim().Split(',');
                    if (cols.Length == 3)
                    {
                        byte[] contents = Properties.Resources.ResourceManager.GetObject(cols[0]) as byte[];
                        string filename = Path.Combine(folder, cols[2]);
                        progressOutput.Report("Copying: " + filename);
                        Directory.CreateDirectory(Path.GetDirectoryName(filename));
                        var fileStream = File.Create(filename);
                        fileStream.Write(contents, 0, contents.Length);
                        fileStream.Close();
                        if (Path.GetExtension(filename).ToLower().Equals(".zip"))
                        {
                            progressOutput.Report("Unzipping: " + filename);
                            System.IO.Compression.ZipFile.ExtractToDirectory(filename, Path.GetDirectoryName(filename));
                            progressOutput.Report("Deleting: " + filename);
                            File.Delete(filename);
                        }
                    }
                }

                // Putting Addin into Excel
                progressStep.Report("Ensuring Excel is closed");
                ExcelHelper.ExcelRunningCheck();
                progressStep.Report("Removing old QuantSA addins");
                ExcelHelper.RemoveAddinsFromExcel();
                progressStep.Report("Putting Addin into Excel");
                ExcelHelper.AddAddinsToExcel(folder);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
