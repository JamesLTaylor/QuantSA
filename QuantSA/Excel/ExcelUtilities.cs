using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Excel
{

    /// <summary>
    /// Functions that will be used in many places when gettting data ready to send to and from Excel
    /// </summary>
    public class ExcelUtilities
    {
        public static Exception latestException = null;

        public static void SetLatestException(Exception e)
        {
            latestException = e;
        }

        /// <summary>
        /// Converts a double reflecting an Excel date into a <see cref="Date"/>.
        /// </summary>
        /// <param name="excelDate">A value reflecting an Excel date.</param>        
        /// <returns></returns>
        public static Date GetDates(double excelDate)
        {
            return new Date(DateTime.FromOADate(excelDate));            
        }

        /// <summary>
        /// Converts doubles reflecting Excel dates into <see cref="Date"/>s
        /// </summary>
        /// <param name="excelDates">Vector of values reflecting Excel dates.</param>
        /// <returns></returns>
        public static Date[] GetDates(double[] excelDates)
        {
            Date[] result = new Date[excelDates.Length];
            for (int i=0; i<excelDates.Length; i++)
            {
                result[i] = new Date(DateTime.FromOADate(excelDates[i]));
            }
            //return excelDates.Select(date => new Date(DateTime.FromOADate(date))).ToArray();
            //TODO: RJ: loop or select
            return result;
        }

        /// <summary>
        /// Convert a string to a tenor object
        /// </summary>
        /// <param name="tenor">String describing a tenor object.  Example '3M' or '5Y'.</param>
        /// <returns></returns>
        public static Tenor GetTenor(object tenor)
        {
            string tenorStr = tenor as string;
            if (tenorStr == null) throw new ArgumentException("The tenor must be provided as a string like: '3M' or '5Y'");
            string numberStr = "";
            int years = 0;
            int months = 0;
            int weeks = 0;
            int days = 0;
            foreach (char c in tenorStr.ToUpper())
            {
                if (c>=48 && c<=57)
                {
                    numberStr += c;
                }
                else if (c=='Y')
                {
                    years = Int32.Parse(numberStr);
                    numberStr = "";
                }
                else if (c == 'M')
                {
                    months = Int32.Parse(numberStr);
                    numberStr = "";
                }
                else if (c == 'W')
                {
                    weeks = Int32.Parse(numberStr);
                    numberStr = "";
                }
                else if (c == 'D')
                {
                    days = Int32.Parse(numberStr);
                    numberStr = "";
                }
                else
                {
                    throw new ArgumentException(tenorStr + " is not a valid tenor String.");
                }
            }
            return new Tenor(days, weeks, months, years);
        }

        /// <summary>
        /// Converts doubles reflecting Excel dates into <see cref="Date"/>s
        /// </summary>
        /// <param name="excelDates">2d array of Excel values.</param>
        /// <returns></returns>
        public static Date[,] GetDates(double[,] excelDates)
        {
            Date[,] result = new Date[excelDates.GetLength(0), excelDates.GetLength(1)];
            for (int i = 0; i < excelDates.GetLength(0); i++)
            {
                for (int j = 0; j < excelDates.GetLength(1); j++)
                {
                    result[i, j] = new Date(DateTime.FromOADate(excelDates[i, j]));
                }
            }
            return result;
        }

        /// <summary>
        /// Convert an erorr message to an object for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object Error0D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return e.Message;
        }

        /// <summary>
        /// Convert an erorr message to a 1d object array for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object[] Error1D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return new object[] { e.Message };
        }

        /// <summary>
        /// Convert an erorr message to a 2d object array for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object[,] Error2D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return new object[,] { { e.Message } };
        }


        /// <summary>
        /// Convert a double array of doubles into objects for returning to excel. 
        /// </summary>
        /// <remarks>
        /// Note that we use objects in Excel return types so that we can send values or strings to the cell. 
        /// </remarks>
        /// <param name="result"></param>
        /// <returns></returns>
        public static object[,] GetObjects(double[,] result)
        {
            object[,] resultObj = new object[result.GetLength(0), result.GetLength(1)];
            for (int i = 0; i<result.GetLength(0); i++ )
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    resultObj[i, j] = result[i, j];
                }
            }
            return resultObj;
        }

        public static int GetInts(double doubleValue, string argName="")
        {
            double intValue = Math.Round(doubleValue);
            if (Math.Abs(doubleValue - intValue) > 1e-10) { throw new ArgumentException(argName + " value must be a whole number"); }
            return (int)intValue;
        }

        /// <summary>
        /// Rounds the values and puts them into an array of ints.  Needed in Excel since taking in ints can be problematic.
        /// </summary>
        /// <param name="doubleArray"></param>
        /// <returns></returns>
        public static int[] GetInts(double[] doubleArray, string argName = "")
        {
            int[] result = new int[doubleArray.Length];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                double intValue = Math.Round(doubleArray[i]);
                if (Math.Abs(doubleArray[i] - intValue) > 1e-10) { throw new ArgumentException(argName + " values must be a whole numbers"); }
                result[i] = (int)intValue;
            }
            return result;
        }

        /// <summary>
        /// Converts an array of <see cref="Date"/> to objects whose values represent Excel dates.
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static object[,] GetObjects(Date[,] dates)
        {
            object[,] result = new object[dates.GetLength(0), dates.GetLength(1)];
            for (int i = 0; i < dates.GetLength(0); i++)
            {
                for (int j = 0; j < dates.GetLength(1); j++)
                {
                    result[i, j] = dates[i,j].date.ToOADate();
                }
            }
            return result;
        }


        /// <summary>
        /// Converts an array of strings to objects.
        /// </summary>
        /// <param name="strValues"></param>
        /// <returns></returns>
        public static object[,] GetObjects(string[,] strValues)
        {
            object[,] result = new object[strValues.GetLength(0), strValues.GetLength(1)];
            for (int i = 0; i < strValues.GetLength(0); i++)
            {
                for (int j = 0; j < strValues.GetLength(1); j++)
                {
                    result[i, j] = strValues[i, j];
                }
            }
            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="boolObject"></param>
        /// <returns></returns>
        public static bool GetBool(object boolObject)
        {
            if (boolObject.ToString().ToUpper().Equals("TRUE"))
            {
                return true;
            }
            if (boolObject.ToString().ToUpper().Equals("FALSE")) { 
                return false;
            }
            throw new ArgumentException("Boolean arguments must be passed as 'TRUE' and 'FALSE'.");
        }

    }
}
