using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using QuantSAInstaller.Properties;

namespace QuantSAInstaller
{
    internal class Installer
    {
        private string _installPath;


        public void Start(string installPath, IProgress<string> progressOutput, IProgress<string> progressStep,
            CancellationToken cancellationToken)
        {
            try
            {
                _installPath = installPath;
                var folder = Path.Combine(_installPath, "QuantSA");
                var installFileInfoRaw = Resources.InstallFileInfo;
                var lines = installFileInfoRaw.Split('\n');

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
                foreach (var line in lines)
                {
                    var cols = line.Trim().Split(',');
                    if (cols.Length != 3) continue;
                    var contents = Resources.ResourceManager.GetObject(cols[0]) as byte[];
                    var filename = Path.Combine(folder, cols[2]);
                    progressOutput.Report("Copying: " + filename);
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    var fileStream = File.Create(filename);
                    fileStream.Write(contents, 0, contents.Length);
                    fileStream.Close();
                    if (Path.GetExtension(filename).ToLower().Equals(".zip"))
                    {
                        progressOutput.Report("Unzipping: " + filename);
                        ZipFile.ExtractToDirectory(filename, Path.GetDirectoryName(filename));
                        progressOutput.Report("Deleting: " + filename);
                        File.Delete(filename);
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