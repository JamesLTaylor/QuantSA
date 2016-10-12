﻿using Microsoft.Win32;
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
    class Program
    {
        static int Main(string[] args)
        {
            Debugger.Launch();

            try
            {
                if (args.Length == 0 || args[0] == "Install")
                {

                    Helper.ExcelRunningCheck();
                    Helper.AddAddinsToExcel(Helper.GetExecutingAssemblyPath());
                    return 0;
                }
                else if (args.Length > 0 && args[0] == "Uninstall")
                {
                    //Don't need to check for Excel here, Installshield will automatically detect that some files are locked and prompt
                    //TODO: Remove the addin from all instance of Excel
                    return 0;
                }
                else
                {
                    throw new Exception("Unknown command parameter " + args[0].ToString());
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}



