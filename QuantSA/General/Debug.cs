using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A collection of Debuging tools, such as writing data to file.  
    /// </summary>
    public class Debug
    {
        /// <summary>
        /// Can be used in debugging to write matrices to a file.
        /// </summary>
        /// <param name="filename">The filename including the path.  The file format will be csv so it
        /// would be convenient to set the extension to .csv but this is not required.</param>
        /// <param name="values">The values.</param>
        public static void WriteToFile(string filename, double[,] values)
        {
            using (StreamWriter sr = new StreamWriter(filename))
            {
                for (int row = 0; row < values.GetLength(0); row++)
                {
                    if (row > 0) sr.Write("\n");
                    for (int col = 0; col < values.GetLength(1); col++)
                    {
                        if (col > 0) sr.Write(",");
                        sr.Write(values[row, col].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Can be used in debugging to write arrays to a file.
        /// </summary>
        /// <param name="filename">The filename including the path.  The file format will be csv so it
        /// would be convenient to set the extension to .csv but this is not required.</param>
        /// <param name="values">The values.</param>
        public static void WriteToFile(string filename, double[] values)
        {
            using (StreamWriter sr = new StreamWriter(filename))
            {
                for (int row = 0; row < values.GetLength(0); row++)
                {
                    if (row > 0) sr.Write("\n");
                    sr.Write(values[row].ToString());
                }
            }
        }
    }
}
