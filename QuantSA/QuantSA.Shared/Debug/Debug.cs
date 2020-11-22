using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace QuantSA.Shared.Debug
{
    /// <summary>
    /// A collection of Debugging tools, such as for writing data to file, writing to output and timing.  
    /// </summary>
    public class Debug
    {
        //TODO: All of these methods should be disabled in a release build. In fact this class could be removed during a release build to ensure that it is not called. 
        private static DateTime _startTime;

        /// <summary>
        /// Can be used in debugging to write matrices to a file.
        /// </summary>
        /// <param name="filename">The filename including the path.  The file format will be csv so it
        /// would be convenient to set the extension to .csv but this is not required.</param>
        /// <param name="values">The values.</param>
        public static void WriteToFile(string filename, double[][] values)
        {
            using (var sr = new StreamWriter(filename))
            {
                for (var row = 0; row < values.Length; row++)
                {
                    if (row > 0) sr.Write("\n");
                    for (var col = 0; col < values[row].Length; col++)
                    {
                        if (col > 0) sr.Write(",");
                        sr.Write(values[row][col].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        /// <summary>
        /// Can be used in debugging to write matrices to a file.
        /// </summary>
        /// <param name="filename">The filename including the path.  The file format will be csv so it
        /// would be convenient to set the extension to .csv but this is not required.</param>
        /// <param name="values">The values.</param>
        public static void WriteToFile(string filename, double[,] values)
        {
            using (var sr = new StreamWriter(filename))
            {
                for (var row = 0; row < values.GetLength(0); row++)
                {
                    if (row > 0) sr.Write("\n");
                    for (var col = 0; col < values.GetLength(1); col++)
                    {
                        if (col > 0) sr.Write(",");
                        sr.Write(values[row, col].ToString(CultureInfo.InvariantCulture));
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
            using (var sr = new StreamWriter(filename))
            {
                for (var row = 0; row < values.GetLength(0); row++)
                {
                    if (row > 0) sr.Write("\n");
                    sr.Write(values[row].ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Starts a timer, there is only one static timer so this can't be used on threads.
        /// </summary>
        public static void StartTimer()
        {
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// returns the elapsed time in milliseconds since <see cref="StartTimer"/> was called.
        /// </summary>
        /// <returns></returns>
        public static double ElapsedTime()
        {
            return (DateTime.Now - _startTime).TotalMilliseconds;
        }

        /// <summary>
        /// Writes a line to <see cref="System.Diagnostics.Trace"/>.  This can be seen in the output window in 
        /// visual studio
        /// </summary>
        /// <param name="value">The string to be displayed.</param>
        public static void WriteLine(string value)
        {
            Trace.WriteLine(value);
        }
    }
}