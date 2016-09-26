using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General;

namespace QuantSA.Excel
{

    /// <summary>
    /// Functions that will be used in many places when gettting data ready to send to and from Excel
    /// </summary>
    public class ExcelUtilities
    {
        /// <summary>
        /// Add an object to the object map.
        /// </summary>
        /// <remarks>Keep this internal in case we need to control how the plugins use the object map.</remarks>
        /// <param name="name">The name of the object on the map.  Must be unique.</param>
        /// <param name="obj">The object to be added.</param>
        /// <returns></returns>
        internal static object AddObject(string name, object obj)
        {
            return ObjectMap.Instance.AddObject(name, obj);
        }

        #region handling errors
        public static Exception latestException = null;

        public static void SetLatestException(Exception e)
        {
            latestException = e;
        }

        /// <summary>
        /// Convert an erorr message to an object for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object Error0D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return "ERROR:" + e.Message;
        }

        /// <summary>
        /// Convert an erorr message to a 1d object array for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object[] Error1D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return new object[] { "ERROR:" + e.Message };
        }

        /// <summary>
        /// Convert an erorr message to a 2d object array for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object[,] Error2D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return new object[,] { { "ERROR:" + e.Message } };
        }
        #endregion

        #region converting return data

        /// <summary>
        /// Convert a double array of doubles into objects for returning to excel. 
        /// </summary>
        /// <remarks>
        /// Note that we use objects in Excel return types so that we can send values or strings to the cell. 
        /// </remarks>
        /// <param name="result"></param>
        /// <returns></returns>
        public static object[,] ConvertToObjects(double[,] result)
        {
            object[,] resultObj = new object[result.GetLength(0), result.GetLength(1)];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    resultObj[i, j] = result[i, j];
                }
            }
            return resultObj;
        }

        /// <summary>
        /// Converts an array of <see cref="Date"/> to objects whose values represent Excel dates.
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static object[,] ConvertToObjects(Date[,] dates)
        {
            object[,] result = new object[dates.GetLength(0), dates.GetLength(1)];
            for (int i = 0; i < dates.GetLength(0); i++)
            {
                for (int j = 0; j < dates.GetLength(1); j++)
                {
                    result[i, j] = dates[i, j].date.ToOADate();
                }
            }
            return result;
        }


        /// <summary>
        /// Converts an array of strings to objects.
        /// </summary>
        /// <param name="strValues"></param>
        /// <returns></returns>
        public static object[,] ConvertToObjects(string[,] strValues)
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

        #endregion

        #region converting input data

        /// <summary>
        /// Get a single <see cref="Date"/> from a single excel date value.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Date GetDates0D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                if (values[0, 0] is double)
                {
                    return new Date(DateTime.FromOADate((double)values[0, 0]));
                }
                throw new ArgumentException(inputName + " must be a single cell with a value representing an Excel Date.");
            }
            throw new ArgumentException(inputName + " must be a single cell with a value representing an Excel Date.");
        }

        /// <summary>
        /// Gets an array of <see cref="Date"/>s from excel date values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Date[] GetDates1D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                Date[] result = new Date[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    if (values[0, i] is double)                         
                        result[i] = new Date(DateTime.FromOADate((double)values[0, i]));
                    else
                        throw new ArgumentException(inputName + " all cells must be values representing a Excel Dates.");
                }
                return result;

            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                Date[] result = new Date[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                {
                    if (values[i, 0] is double)
                        result[i] = new Date(DateTime.FromOADate((double)values[i, 0]));
                    else
                        throw new ArgumentException(inputName + " all cells must be values representing a Excel Dates.");
                }
                return result;
            }
            else
            {
                throw new ArgumentException(inputName + " must be a single row or column of Excel dates.");
            }
        }

        /// <summary>
        /// Gets an 2D array of <see cref="Date"/>s from excel date values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Date[,] GetDates2D(object[,] values, string inputName)
        {
            Date[,] result = new Date[values.GetLength(0), values.GetLength(1)];
            for (int i = 0; i<values.GetLength(0); i++)
            {
                for (int j =0; j<values.GetLength(1); j++)
                {
                    if (values[i, j] is double)
                        result[i, j] = new Date(DateTime.FromOADate((double)values[i, j]));
                    else
                        throw new ArgumentException(inputName + " all cells must be values representing a Excel Dates.");
                }
            }
            return result;
        }

        /// <summary>
        /// Returns an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the object required off the map.</typeparam>
        /// <param name="values">The excel values passed to the function.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static T GetObjects0D<T>(object[,] values, string inputName)
        {
            if (values.GetLength(0) != 1 || values.GetLength(1) != 1) throw new ArgumentException(inputName + " must be a single string refering to an existing object.");
            String name = values[0, 0] as String;
            if (name != null) return ObjectMap.Instance.GetObjectFromID<T>(name);
            throw new ArgumentException(inputName + " must be a single string refering to an existing object.");
        }

        /// <summary>
        /// Returns a list of objects of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the object required off the map.</typeparam>
        /// <param name="values">The excel values passed to the function</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static List<T> GetObjects1D<T>(object[,] values, string inputName)
        {            
            List<T> result = new List<T>();
            if (values[0, 0] is ExcelMissing) return result; // Empty input
            if (values.GetLength(0) > 1 && values.GetLength(1) > 1) // matrix input
            {
                throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
            }
            if (values.GetLength(0) == 1) // row input
            {
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    String name = values[0, i] as String;
                    if (name == null)
                    {
                        throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
                    }
                    result.Add(ObjectMap.Instance.GetObjectFromID<T>(name));
                }
            }
            else if (values.GetLength(1) == 1) // column input
            {
                for (int i = 0; i < values.GetLength(0); i++)
                {
                    String name = values[i, 0] as String;
                    if (name == null)
                    {
                        throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
                    }
                    result.Add(ObjectMap.Instance.GetObjectFromID<T>(name));
                }
            }
            return result;
        }

        /// <summary>
        /// Get a single double from an excel input.
        /// </summary>
        /// <param name="values">The excel values passed to the function.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static double GetDoubles0D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                if (values[0, 0] is double)
                {
                    return (double)values[0, 0];
                }
                throw new ArgumentException(inputName + " must be a single cell with a value.");
            }
            throw new ArgumentException(inputName + " must be a single cell with a value.");

        }

        /// <summary>
        /// Get a 1D array of doubles from an excel input.  Can be a row or a column.
        /// </summary>
        /// <param name="values">The excel values passed to the function.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static double[] GetDoubles1D(object[,] values, string inputName)
        {

            if (values.GetLength(0)==1 && values.GetLength(1)>=1) // row of inputs
            {
                double[] result = new double[values.GetLength(1)];
                for (int i = 0; i< values.GetLength(1); i++)
                {
                    if (values[0, i] is double)
                        result[i] = (double)values[0, i];
                    else
                        throw new ArgumentException(inputName + " must only contain numbers.");
                }
                return result;
                
            } else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                double[] result = new double[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                {
                    if (values[i, 0] is double)
                        result[i] = (double)values[i, 0];
                    else
                        throw new ArgumentException(inputName + " must only contain numbers.");
                }
                return result;

            } else
            {
                throw new ArgumentException(inputName + " must be a single row or column of values.");
            }
        }

        /// <summary>
        /// Used by the various GetCurrencies utility methods.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static Currency GetCurrency(object obj, string inputName, bool isOptional)
        {
            if (obj is ExcelMissing)
                if (isOptional)
                    return Currency.ANY;
                else
                    throw new ArgumentException(inputName + " cannot be empty.");
            if (obj is string)
            {
                string strValue = (string)obj;
                switch (strValue.ToUpper())
                {
                    case "ZAR": return Currency.ZAR;
                    case "USD": return Currency.USD;
                    case "EUR": return Currency.EUR;
                    default: throw new ArgumentException(strValue + " is not a known currency in input: " + inputName);
                }
            }
            else
                throw new ArgumentException(inputName + ": Currencies must be strings: ");


        }

        /// <summary>
        /// Get a <see cref="Currency"/> from a string.
        /// </summary>
        /// <remarks>
        /// This is implemented in the Excel layer rather than in <see cref="Currency"/> itself to make sure that users in the library don't use strings to construct things.
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Currency GetCurrencies0D(object[,] values, string inputName, bool isOptional=false)
        {            
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                return GetCurrency(values[0, 0], inputName, isOptional);
            }
            throw new ArgumentException(inputName + " must be a single cell with a string representing a currency.");
        }

        /// <summary>
        /// Get an array of <see cref="Currency"/> from an excel range of strings.
        /// </summary>
        /// <remarks>
        /// This is implemented in the Excel layer rather than in <see cref="Currency"/> itself to make sure that users in the library don't use strings to construct things.
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Currency[] GetCurrencies1D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                Currency[] result = new Currency[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                    result[i] = GetCurrency(values[0, i], inputName, false);
                return result;
            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                Currency[] result = new Currency[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                    result[i] = GetCurrency(values[i, 0], inputName, false);
                return result;
            }
            else
                throw new ArgumentException(inputName + " must be a single row or column of strings representing currencies.");
        }

        /// <summary>
        /// Get a <see cref="FloatingIndex"/> from a string.
        /// </summary>
        /// <remarks>
        /// This is implemented in the Excel layer rather than in <see cref="FloatingIndex"/> itself to make sure that users in the library don't use strings to construct things.
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static FloatingIndex GetFloatingIndices0D(object[,] values, string inputName)
        {
            if (values[0, 0] is ExcelMissing) throw new ArgumentException(inputName + " must be a single cell with a string representing a floating rate index.");
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                string strValue = (string)values[0, 0];
                switch (strValue.ToUpper())
                {
                    case "JIBAR3M": return FloatingIndex.JIBAR3M;
                    case "JIBAR6M": return FloatingIndex.JIBAR6M;
                    case "LIBOR3M": return FloatingIndex.LIBOR3M;
                    default: throw new ArgumentException(strValue + "is not a known floating rate index in input: " + inputName);
                }
            }
            throw new ArgumentException(inputName + " must be a single cell with a string representing a floating rate index.");
        }


        /// <summary>
        /// Convert a string to a tenor object
        /// </summary>
        /// <remarks>Implemented here to make sure that users in the libary don't construct with strings.</remarks>
        /// <param name="values">String describing a tenor object.  Example '3M' or '5Y'.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>/// 
        /// <returns></returns>
        public static Tenor GetTenors0D(object[,] values, string inputName)
        {
            if (values.GetLength(0) > 1 || values.GetLength(1) > 1) throw new ArgumentException(inputName + " must be a single string describing a tenor.  Eg '3M' or '5Y'");
            string tenorStr = values[0,0] as string;
            if (tenorStr == null) throw new ArgumentException(inputName + " must be provided as a string like: '3M' or '5Y'");
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

        private static int GetInt(object obj, string inputName)
        {
            if (obj is ExcelMissing)
                throw new ArgumentException(inputName + " cannot be empty.");
            if (obj is double)
            {
                double doubleValue = (double)obj;
                double intValue = Math.Round(doubleValue);
                if (Math.Abs(doubleValue - intValue) > 1e-10) { throw new ArgumentException(inputName + " cannot contain fractions."); }
                return (int)intValue;
            }
            throw new ArgumentException(inputName + " must have numbers.");
        }


        /// <summary>
        /// Get a single int value from an Excel number.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static int GetInts0D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                return GetInt(values[0, 0], inputName);
            }
            throw new ArgumentException(inputName + " must be a single cell with a whole number");
        }

        /// <summary>
        /// Gets an array of <see cref="int"/>s from whole number is Excel.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static int[] GetInts1D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                int[] result = new int[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    result[i] = GetInt(values[0, i], inputName);
                }
                return result;
            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                int[] result = new int[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                {                 
                    result[i] = GetInt(values[i, 0], inputName);
                }
                return result;
            }
            else
            {
                throw new ArgumentException(inputName + " must be a single row or column of whole numbers.");
            }
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

    #endregion 
}
