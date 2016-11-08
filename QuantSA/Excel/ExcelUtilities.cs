using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using QuantSA.General;

namespace QuantSA.Excel
{

    /// <summary>
    /// Functions that will be used in many places when gettting data ready to send to and from Excel
    /// </summary>
    public class ExcelUtilities
    {

        /// <summary>
        /// Determines whether the output type of a function is primitive and will be written deirectly 
        /// to the cells or is not primitive and will be placed on the object map and a reference returned 
        /// to the cells.
        /// </summary>
        /// <param name="outputType">Type of the output.</param>
        /// <returns>
        ///   <c>true</c> if the output type is QuantSA primitive; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrimitiveOutput(Type outputType)
        {
            Type type = outputType.IsArray ? outputType.GetElementType() : outputType;
            if (type == typeof(double)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(Date)) return true;
            return false;

        }


        /// <summary>
        /// Does the input type have an automatic conversion from Excel cell values.  If it does not
        /// then an object will need to be retrieved from the object map.
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        /// <returns></returns>
        public static bool InputTypeHasConversion(Type inputType)
        {
            Type type = inputType.IsArray ? inputType.GetElementType() : inputType;
            if (type == typeof(double)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(Date)) return true;
            if (type == typeof(Currency)) return true;
            if (type == typeof(FloatingIndex)) return true;
            if (type == typeof(Tenor)) return true;
            if (type == typeof(Share)) return true;
            return false;
        }

        /// <summary>
        /// Should the input of this type include a link to the help about that type?  For example
        /// if the input type is <see cref="FloatingIndex"/> then it is useful to link to the
        /// page on FloatingIndex so that the user can see the permissable strings.
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        /// <returns></returns>
        public static bool InputTypeShouldHaveHelpLink(Type inputType)
        {
            Type type = inputType.IsArray ? inputType.GetElementType() : inputType;
            if (type == typeof(bool)) return true;
            if (type == typeof(Date)) return true;
            if (type == typeof(Currency)) return true;
            if (type == typeof(FloatingIndex)) return true;
            if (type == typeof(Tenor)) return true;
            if (type == typeof(Share)) return true;
            return false;
        }



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
            return "ERROR: " + e.Message;
        }

        /// <summary>
        /// Convert an erorr message to a 1d object array for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object[] Error1D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return new object[] { "ERROR: " + e.Message };
        }

        /// <summary>
        /// Convert an erorr message to a 2d object array for return to Excel.
        /// </summary>
        /// <param name="e">The exception that has been thrown and needs to be displaced in the cell.</param>
        /// <returns></returns>
        public static object[,] Error2D(Exception e)
        {
            ExcelUtilities.SetLatestException(e);
            return new object[,] { { "ERROR: " + e.Message } };
        }
        #endregion

        #region converting return data

        /// <summary>
        /// Convert an array of doubles into objects for returning to excel. 
        /// </summary>
        /// <remarks>
        /// Note that we use objects in Excel return types so that we can send values or strings to the cell. 
        /// </remarks>
        /// <param name="result"></param>
        /// <returns></returns>
        public static object[,] ConvertToObjects(double[] result, bool asColumn)
        {
            if (asColumn)
            {
                object[,] resultObj = new object[result.Length, 1];
                for (int i = 0; i < result.Length; i++)
                {
                    resultObj[i, 0] = result[i];
                }
                return resultObj;
            }
            else
            {
                object[,] resultObj = new object[1, result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    resultObj[0, i] = result[i];
                }
                return resultObj;
            }
            
        }

        /// <summary>
        /// Convert a double array of doubles into objects for returning to excel. 
        /// </summary>
        /// <remarks>
        /// Note that we use objects in Excel return types so that we can send values or strings to the cell. 
        /// </remarks>
        /// <param name="result"></param>
        /// <returns></returns>
        public static object[] ConvertToObjects(double[] result)
        {
            object[] resultObj = new object[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                resultObj[i] = result[i];
            }
            return resultObj;
        }


        /// <summary>
        /// Convert a 2d array of doubles into objects for returning to excel. 
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
        public static Date GetDate0D(object[,] values, string inputName)
        {
            if (values[0, 0] is ExcelMissing) throw new ArgumentException(inputName + " cannot be empty.");
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
        public static Date[] GetDate1D(object[,] values, string inputName)
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
        public static Date[,] GetDate2D(object[,] values, string inputName)
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
        public static T GetObject0D<T>(object[,] values, string inputName)
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
        public static T[] GetObject1D<T>(object[,] values, string inputName)
        {            
            if (values[0, 0] is ExcelMissing) return new T[0]; // Empty input
            if (values.GetLength(0) > 1 && values.GetLength(1) > 1) // matrix input
            {
                throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
            }
            if (values.GetLength(0) == 1) // row input
            {
                T[] result = new T[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    String name = values[0, i] as String;
                    if (name == null)
                    {
                        throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
                    }
                    result[i] = ObjectMap.Instance.GetObjectFromID<T>(name);
                }
                return result;
            }
            else if (values.GetLength(1) == 1) // column input
            {
                T[] result = new T[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                {
                    String name = values[i, 0] as String;
                    if (name == null)
                    {
                        throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
                    }
                    result[i] = ObjectMap.Instance.GetObjectFromID<T>(name);
                }
                return result;
            }
            else
            {
                throw new ArgumentException(inputName + " must be single row or column of strings referring to existing objects");
            }            
        }

        /// <summary>
        /// Get a single double from an excel input.
        /// </summary>
        /// <param name="values">The excel values passed to the function.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static double GetDouble0D(object[,] values, string inputName)
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
        public static double[] GetDouble1D(object[,] values, string inputName)
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
        /// 
        /// </summary>
        /// <param name="values">The excel values passed to the function.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static double[,] GetDouble2D(object[,] values, string inputName)
        {
            if (values[0, 0] is ExcelMissing) throw new ArgumentException(inputName + " + cannot be empty.");
            double[,] result = new double[values.GetLength(0), values.GetLength(1)];
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (values[i, j] is double)
                        result[i, j] = (double)values[i, j];
                    else
                        throw new ArgumentException(inputName + " all cells must be numbers.");
                }
            }
            return result;
        }


        public static Currency GetCurrencyFromString(string strValue, string inputName)
        {
            switch (strValue.ToUpper())
            {
                case "ZAR": return Currency.ZAR;
                case "USD": return Currency.USD;
                case "EUR": return Currency.EUR;
                default: throw new ArgumentException(strValue + " is not a known currency in input: " + inputName);
            }
        }


        /// <summary>
        /// Used by the various GetCurrencies utility methods.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static Currency GetCurrency(object obj, string inputName, Currency defaultValue = null)
        {
            if (obj is ExcelMissing)
                if (defaultValue!=null)
                    return defaultValue;
                else
                    throw new ArgumentException(inputName + " cannot be empty.");
            if (obj is string)
            {
                return GetCurrencyFromString((string)obj, inputName);
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
        public static Currency GetCurrency0D(object[,] values, string inputName, Currency defaultValue = null)
        {            
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                return GetCurrency(values[0, 0], inputName, defaultValue);
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
        public static Currency[] GetCurrency1D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                Currency[] result = new Currency[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                    result[i] = GetCurrency(values[0, i], inputName, null);
                return result;
            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                Currency[] result = new Currency[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                    result[i] = GetCurrency(values[i, 0], inputName, null);
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
        public static FloatingIndex GetFloatingIndex0D(object[,] values, string inputName)
        {
            if (values[0, 0] is ExcelMissing) throw new ArgumentException(inputName + " must be a single cell with a string representing a floating rate index.");
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                string strValue = (string)values[0, 0];
                switch (strValue.ToUpper())
                {
                    case "JIBAR1M": return FloatingIndex.JIBAR1M;
                    case "JIBAR3M": return FloatingIndex.JIBAR3M;
                    case "JIBAR6M": return FloatingIndex.JIBAR6M;
                    case "PRIME1M_AVG": return FloatingIndex.PRIME1M_AVG;
                    case "LIBOR1M": return FloatingIndex.LIBOR1M;
                    case "LIBOR3M": return FloatingIndex.LIBOR3M;
                    case "LIBOR6M": return FloatingIndex.LIBOR6M;
                    case "EURIBOR3M": return FloatingIndex.EURIBOR3M;
                    case "EURIBOR6M": return FloatingIndex.EURIBOR6M;
                    default: throw new ArgumentException(strValue + " is not a known floating rate index in input: " + inputName);
                }
            }
            throw new ArgumentException(inputName + " must be a single cell with a string representing a floating rate index.");
        }


        private static Tenor GetTenorFromString(string strValue, string inputName)
        {
            string numberStr = "";
            int years = 0;
            int months = 0;
            int weeks = 0;
            int days = 0;
            foreach (char c in strValue.ToUpper())
            {
                if (c >= 48 && c <= 57)
                {
                    numberStr += c;
                }
                else if (c == 'Y')
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
                    throw new ArgumentException(strValue + " is not a valid tenor String.");
                }
            }
            return new Tenor(days, weeks, months, years);
        }



        /// <summary>
        /// Used by the various GetTenor utility methods.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static Tenor GetTenor(object obj, string inputName)
        {
            if (obj is ExcelMissing)
                    throw new ArgumentException(inputName + " cannot be empty.");
            if (obj is string)
            {
                return GetTenorFromString((string)obj, inputName);
            }
            else
                throw new ArgumentException(inputName + ": Tenors must be provided as strings with a number and a period identifier. e.g. '3M' or '5Y' ");
        }


        /// <summary>
        /// Convert a string to a tenor object
        /// </summary>
        /// <remarks>Implemented here to make sure that users in the libary don't construct with strings.</remarks>
        /// <param name="values">String describing a tenor object.  Example '3M' or '5Y'.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>/// 
        /// <returns></returns>
        public static Tenor GetTenor0D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                return GetTenor(values[0, 0], inputName);
            }
            throw new ArgumentException(inputName + " must be a single cell with a string representing a tenor. e.g. '3M' or '5Y'");

        }

        /// <summary>
        /// Get an array of <see cref="Tenor"/> from an excel range of strings.
        /// </summary>
        /// <remarks>
        /// This is implemented in the Excel layer rather than in <see cref="Tenor"/> itself to make sure that users in the library don't use strings to construct things.
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Tenor[] GetTenor1D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                Tenor[] result = new Tenor[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                    result[i] = GetTenor(values[0, i], inputName);
                return result;
            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                Tenor[] result = new Tenor[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                    result[i] = GetTenor(values[i, 0], inputName);
                return result;
            }
            else
                throw new ArgumentException(inputName + " must be a single row or column of strings representing tenors.");
        }

        /// <summary>
        /// Return a share from a string of form 'ZAR:ALSI'
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static Share GetShare(object obj, string inputName)
        {
            if (obj is ExcelMissing)
                throw new ArgumentException(inputName + " cannot be empty.");
            if (obj is string)
            {
                string strValue = (string)obj;
                string[] parts = strValue.Split(':');
                if (parts.Length != 2) throw new ArgumentException(strValue + " in " + inputName + " does not correspond to a share.");
                Currency ccy = GetCurrencyFromString(parts[0], inputName);
                return new Share(parts[1].ToUpper(), ccy);
            }
            throw new ArgumentException(inputName + " shares must be created from strings of the form: 'CCY:CODE', eg 'ZAR:ALSI'");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Share GetShare0D(object[,] values, string inputName)
        {
            if (values[0, 0] is ExcelMissing)
                throw new ArgumentException(inputName + " cannot be empty.");
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                if (values[0, 0] is string)
                {
                    return GetShare((string)values[0, 0], inputName);
                }
                throw new ArgumentException(inputName + " must be a single cell with a value representing a share code.");
            }
            throw new ArgumentException(inputName + " must be a single cell with a value representing a share code.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Share[] GetShare1D(object[,] values, string inputName)
        {
            if (values[0, 0] is ExcelMissing) throw new ArgumentException(inputName + " cannot be empty.");
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                Share[] result = new Share[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                    result[i] = GetShare(values[0, i], inputName);
                return result;
            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                Share[] result = new Share[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                    result[i] = GetShare(values[i, 0], inputName);
                return result;
            }
            else
                throw new ArgumentException(inputName + " must be a single row or column of strings representing shares.");
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
        public static int GetInt320D(object[,] values, string inputName, int? defaultValue=null)
        {
            if (values[0, 0] is ExcelMissing){
                if (defaultValue == null)
                    throw new ArgumentException("input: '" + inputName + "' is left out and is not optional");
                else
                    return (int)defaultValue;
            }
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
        public static int[] GetInt321D(object[,] values, string inputName)
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
        public static bool GetBoolean0D(object[,] values, string inputName)
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                if (values[0,0].ToString().ToUpper().Equals("TRUE"))
                {
                    return true;
                }
                if (values[0,0].ToString().ToUpper().Equals("FALSE"))
                {
                    return false;
                }
                throw new ArgumentException("Boolean arguments must be passed as 'TRUE' and 'FALSE'.");
            }
            throw new ArgumentException(inputName + " must be a single cell with a whole number");
            
        }
    }

    #endregion 
}
