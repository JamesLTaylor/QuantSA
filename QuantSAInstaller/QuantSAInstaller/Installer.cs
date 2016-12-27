using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuantSAInstaller
{
    class Installer
    {
        string outputContent;

        public void Start(string installPath, IProgress<string> progressOutput, IProgress<string> progressStep, CancellationToken cancellationToken)
        {
            string installFileInfoRaw = Properties.Resources.InstallFileInfo;
            string[] lines = installFileInfoRaw.Split('\n');
            string[,] installFileInfo = new string[lines.Length, 3];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(',');
                installFileInfo[i, 0] = cols[0];
                installFileInfo[i, 1] = cols[1];
                installFileInfo[i, 2] = cols[2];
            }

            outputContent = "Started";
            progressOutput.Report(outputContent);
            outputContent += "\nExtracting Files";
            progressOutput.Report(outputContent);
            progressStep.Report("Installing Files");

            byte[] contents = Properties.Resources.ResourceManager.GetObject("BermudanSwaption") as byte[];
            var fileStream = File.Create("C:\\temp\\new_bermudan.xlsx");
            fileStream.Write(contents, 0, contents.Length);
            fileStream.Close();
        }

        
    }
}
