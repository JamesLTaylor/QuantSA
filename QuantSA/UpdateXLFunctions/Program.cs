using Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UpdateXLFunctions
{
    /// <summary>
    /// Writes all the functions exposed in Excel.dll to a file/
    /// </summary>
    class Program
    {
        const string FunctionsFilenameAll = "functions_all.csv";
        
        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Dictionary<string, bool> funcsInDll = global::MyAddIn.GetQuantSAFunctionVisibility();
            List<string> list = funcsInDll.Keys.ToList();
            list.Sort();
            using (StreamWriter file = new StreamWriter(FunctionsFilenameAll))
            {
                foreach (string key in list)
                {
                    file.WriteLine(key + "," + (funcsInDll[key]?"yes":"no"));
                }
            }
        }
    }
}
