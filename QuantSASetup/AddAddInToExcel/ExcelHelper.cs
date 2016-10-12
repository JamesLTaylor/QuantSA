using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuantSA.Setup.CustomActions
{
    static class Helper
    {
        private static int minOfficeVersion = 12;
        private static int maxOfficeVersion = 16;

        private static bool is64BitProcess = (IntPtr.Size == 8);
        public static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        private static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        public static List<BitInstance> GetSupportedExcelVersions(string registrySearchPath)
        {
            List<BitInstance> results = new List<BitInstance>();

            int officeVer = minOfficeVersion;

            while (officeVer <= maxOfficeVersion)
            {
                string officeRegistryPath = registrySearchPath + officeVer.ToString("#.0", CultureInfo.InvariantCulture);

                RegistryKey officeRegistryKey = Registry.CurrentUser.OpenSubKey(officeRegistryPath, true);

                if (officeRegistryKey != null)
                {
                    string optionsRegistryPath = officeRegistryPath + @"\Excel\Options";
                    RegistryKey optionsRegistryKey = Registry.CurrentUser.OpenSubKey(optionsRegistryPath, true);

                    if (optionsRegistryKey != null)
                    {
                        Bitness excelBitness;

                        if (is64BitOperatingSystem)
                        {
                            //TODO: Figure out what the bitness of excel is. Assume 32 for now
                            excelBitness = Bitness.Bitness32;
                        }
                        else
                        {
                            excelBitness = Bitness.Bitness32;
                        }

                        results.Add(new BitInstance(optionsRegistryPath, excelBitness));
                    }
                }

                officeVer++;
            }

            return results;
        }

        public static void AddAddinsToExcel(string targetPath)
        {
            string officeRegistryKey = @"Software\Microsoft\Office\";

            foreach (var excelInstance in GetSupportedExcelVersions(officeRegistryKey))
            {
                InstallRelevantAddin(targetPath, excelInstance);
            }

        }

        private static void InstallRelevantAddin(string targetPath, BitInstance excelInstance)
        {
            var view = is64BitOperatingSystem ?
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64) :
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

            RegistryKey officeRegistryKey = view.OpenSubKey(excelInstance.Data, true);
            FileInfo addIn;

            if (excelInstance.Bitness == Bitness.Bitness32)
            {
                addIn = new FileInfo(Path.Combine(targetPath, "QuantSA.xll"));
            }
            else
            {
                addIn = new FileInfo(Path.Combine(targetPath, "QuantSA64.xll"));
            }

            InstallAddin(officeRegistryKey, addIn);
        }

        private static void InstallAddin(RegistryKey key, FileInfo addIn)
        {
            //We have to search and find the next available "Open" key
            int openNumber = 0;
            bool added = false;

            while (!added)
            {
                string openString = (openNumber != 0) ?
                    openNumber.ToString() :
                    null;

                string openValue = "OPEN" + openString;
                object openKeyValue = key.GetValue(openValue);

                if (openKeyValue == null)
                {
                    key.SetValue(openValue, "\"" + addIn.FullName + "\"", RegistryValueKind.String);
                    added = true;
                }
                else if (openKeyValue.ToString() == "\"" + addIn.FullName + "\"") // The add-in is already added, update the path
                {
                    key.SetValue(openValue, "\"" + addIn.FullName + "\"", RegistryValueKind.String);
                    added = true;
                }
                else
                {
                    openNumber++;
                }
            }
        }

        public static void ExcelRunningCheck()
        {
            // Check if Excel is running and prompt the user to retry or cancel
            DialogResult dialogResult = DialogResult.Retry;

            while ((Process.GetProcessesByName("Excel").Length > 0) && (dialogResult == DialogResult.Retry))
            {
                dialogResult = MessageBox.Show("The installer cannot continue while Excel is running, " +
                    "please close all Excel instances and press Retry.",
                    "Close Excel",
                    MessageBoxButtons.RetryCancel);
            }

            if (dialogResult == DialogResult.Cancel)
            {
                throw new Exception("Installation cancelled by the user.");
            }
        }

        public static string GetExecutingAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);

            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)); ;
        }

    }
}
