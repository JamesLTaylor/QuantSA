using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;

namespace QuantSAInstaller
{
    internal static class ExcelHelper
    {
        private static readonly string officeRegistryKey = @"Software\Microsoft\Office\";
        private static readonly int minOfficeVersion = 12;
        private static readonly int maxOfficeVersion = 16;

        private static readonly bool is64BitProcess = IntPtr.Size == 8;
        public static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );


        private static bool InternalCheckIsWow64()
        {
            if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1 ||
                Environment.OSVersion.Version.Major >= 6)
                using (var p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal)) return false;
                    return retVal;
                }

            return false;
        }

        public static List<BitInstance> GetSupportedExcelVersions(string registrySearchPath)
        {
            var results = new List<BitInstance>();

            var officeVer = minOfficeVersion;

            while (officeVer <= maxOfficeVersion)
            {
                var officeRegistryPath = registrySearchPath + officeVer.ToString("#.0", CultureInfo.InvariantCulture);

                var officeRegistryKey = Registry.CurrentUser.OpenSubKey(officeRegistryPath, true);

                if (officeRegistryKey != null)
                {
                    var optionsRegistryPath = officeRegistryPath + @"\Excel\Options";
                    var optionsRegistryKey = Registry.CurrentUser.OpenSubKey(optionsRegistryPath, true);

                    if (optionsRegistryKey != null)
                    {
                        Bitness excelBitness;

                        if (is64BitOperatingSystem)
                            excelBitness = Bitness.Bitness32;
                        else
                            excelBitness = Bitness.Bitness32;

                        results.Add(new BitInstance(optionsRegistryPath, excelBitness));
                    }
                }

                officeVer++;
            }

            return results;
        }

        internal static void AddAddinsToExcel(string targetPath)
        {
            foreach (var excelInstance in GetSupportedExcelVersions(officeRegistryKey))
                InstallRelevantAddin(targetPath, excelInstance);
        }

        internal static void RemoveAddinsFromExcel()
        {
            foreach (var excelInstance in GetSupportedExcelVersions(officeRegistryKey))
                RemoveRelevantAddin(excelInstance);
        }

        private static void InstallRelevantAddin(string targetPath, BitInstance excelInstance)
        {
            var view = is64BitOperatingSystem
                ? RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64)
                : RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

            var officeRegistryKey = view.OpenSubKey(excelInstance.Data, true);
            FileInfo addIn;

            if (excelInstance.Bitness == Bitness.Bitness32)
                addIn = new FileInfo(Path.Combine(targetPath, "QuantSA.xll"));
            else
                addIn = new FileInfo(Path.Combine(targetPath, "QuantSA64.xll"));

            InstallAddin(officeRegistryKey, addIn);
        }

        private static void RemoveRelevantAddin(BitInstance excelInstance)
        {
            var view = is64BitOperatingSystem
                ? RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64)
                : RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

            var officeRegistryKey = view.OpenSubKey(excelInstance.Data, true);
            string addIn;

            if (excelInstance.Bitness == Bitness.Bitness32)
                addIn = "QuantSA.xll";
            else
                addIn = "QuantSA64.xll";

            RemoveAddin(officeRegistryKey, addIn);
        }

        private static void InstallAddin(RegistryKey key, FileInfo addIn)
        {
            //We have to search and find the next available "Open" key
            var openNumber = 0;
            var added = false;

            while (!added)
            {
                var openString = openNumber != 0 ? openNumber.ToString() : null;

                var openValue = "OPEN" + openString;
                var openKeyValue = key.GetValue(openValue);

                if (openKeyValue == null)
                {
                    key.SetValue(openValue, "\"" + addIn.FullName + "\"", RegistryValueKind.String);
                    added = true;
                }
                else if (openKeyValue.ToString() == "\"" + addIn.FullName + "\""
                ) // The add-in is already added, update the path
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

        private static void RemoveAddin(RegistryKey key, string addIn)
        {
            foreach (var value in key.GetValueNames())
                if (value.StartsWith("Open") && value.Contains(addIn))
                    key.DeleteValue(value);
        }

        public static void ExcelRunningCheck()
        {
            // Check if Excel is running and prompt the user to retry or cancel
            var dialogResult = MessageBoxResult.OK;

            while (Process.GetProcessesByName("Excel").Length > 0 && dialogResult == MessageBoxResult.OK)
                dialogResult = MessageBox.Show("The installer cannot continue while Excel is running, " +
                                               "please close all Excel instances and press Retry.",
                    "Close Excel", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK);

            if (dialogResult == MessageBoxResult.Cancel) throw new Exception("Installation cancelled by the user.");
        }

        public static string GetExecutingAssemblyPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);

            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }
    }
}