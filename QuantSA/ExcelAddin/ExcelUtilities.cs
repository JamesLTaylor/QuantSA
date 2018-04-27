using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Excel.Common;
using QuantSA.General.Conventions.Compounding;
using QuantSA.Primitives.Dates;
using QuantSA.ExcelFunctions;
using QuantSA.General.Conventions.BusinessDay;
using QuantSA.General.Conventions.DayCount;
using QuantSA.General.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.Excel
{

    /// <summary>
    /// Functions that will be used in many places when gettting data ready to send to and from Excel
    /// </summary>
    public class ExcelUtilities
    {
        /// <summary>
        /// Should the input of this type include a link to the help about that type?  For example
        /// if the input type is <see cref="FloatingIndex"/> then it is useful to link to the
        /// page on FloatingIndex so that the user can see the permissable strings.
        /// </summary>
        /// <remarks>
        /// This method is a copy of PrepareRelease.MarkdownGenerator.InputTypeShouldHaveHelpLink        /// 
        /// </remarks>
        /// <param name="inputType">Type of the input.</param>
        /// <returns></returns>
        internal static bool InputTypeShouldHaveHelpLink(Type inputType)
        {
            //TODO: This method should be replaced with checking if the type is in documentedTypes.  That would mean one less thing to maintain.
            Type type = inputType.IsArray ? inputType.GetElementType() : inputType;
            if (type == typeof(bool)) return true;
            if (type.Name == "Date") return true;
            if (type.Name == "Currency") return true;
            if (type.Name == "FloatingIndex") return true;
            if (type.Name == "Tenor") return true;
            if (type.Name == "Share") return true;
            if (type.Name == "ReferenceEntity") return true;
            if (type.Name == "CompoundingConvention") return true;
            if (type.Name == "DayCountConvention") return true;
            if (type.Name == "BusinessDayConvention") return true;
            if (type.Name == "Calendar") return true;
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
        /// Convert an single double into a 2d array of objects for returning to excel. 
        /// </summary>
        /// <remarks>
        /// Note that we use objects in Excel return types so that we can send values or strings to the cell. 
        /// </remarks>
        /// <param name="result"></param>
        /// <returns></returns>
        public static object[,] ConvertToObjects(double result)
        {
            object[,] resultObj = new object[1, 1];
            resultObj[0, 0] = result;
            return resultObj;            
        }

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
        public static object[,] ConvertToObjects(double[] result)
        {
            return ConvertToObjects(result, true);
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
        public static object[,] ConvertToObjects(Date date)
        {
            return ConvertToObjects(new Date[,] { { date } });            
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
                    result[i, j] = dates[i, j].ToOADate();
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a string to a 2d array of objects.
        /// </summary>
        /// <param name="strValues"></param>
        /// <returns></returns>
        public static object[,] ConvertToObjects(string strValue)
        {
            object[,] result = new object[1,1];
            result[0, 0] = strValue;            
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
        /// Get a single <see cref="Date"/> from a single excel date value.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static Date GetDate0D(object[,] values, string inputName, Date defaultValue)
        {
            if (values[0, 0] is ExcelMissing) return defaultValue;
            else return GetDate0D(values, inputName);            
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
        /// Returns an object of type <typeparamref name="T"/> but allows for a default value should values not be set.
        /// </summary>
        /// <typeparam name="T">The type of the object required off the map.</typeparam>
        /// <param name="values">The excel values passed to the function.</param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <param name="defaultValue">The default value to return if the excel input is missing.  Can be null.</param>
        /// <returns></returns>
        public static T GetObject0D<T>(object[,] values, string inputName, object defaultValue)
        {
            if (values[0, 0] is ExcelMissing)
                return (T)defaultValue;
            else
                return GetObject0D<T>(values, inputName);
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

        public static double GetDouble0D(object[,] values, string inputName, double defaultValue)
        {
            if (values[0,0] is ExcelMissing) return defaultValue;
            return GetDouble0D(values, inputName);
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


        public static String GetStringFromString(string strValue, string inputName)
        {
            return strValue;
        }

        /// <summary>
        /// Return a share from a string of form 'ZAR:ALSI'
        /// </summary>
        /// <param name="strValue">The string value.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        private static Share GetShareFromString(string strValue, string inputName)
        {
            string[] parts = strValue.Split(':');
            if (parts.Length != 2) throw new ArgumentException(strValue + " in " + inputName + " does not correspond to a share.");
            Currency ccy = GetCurrencyFromString(parts[0], inputName);
            return new Share(parts[1].ToUpper(), ccy);
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
        

        public static CompoundingConvention GetCompoundingConventionFromString(string strValue, string inputName)
        {
            switch (strValue.ToUpper())
            {                
                case "SIMPLE": return CompoundingStore.Simple;
                case "DISCOUNT": return CompoundingStore.Discount;
                case "C":
                case "NACC":
                case "CONTINUOUS": return CompoundingStore.Continuous;
                case "D":
                case "NACD":
                case "DAILY": return CompoundingStore.Daily;
                case "M":
                case "NACM":
                case "MONTHLY": return CompoundingStore.Monthly;
                case "Q":
                case "NACQ":
                case "QUARTERLY": return CompoundingStore.Quarterly;
                case "S":
                case "NACS":
                case "SEMIANNUAL": return CompoundingStore.SemiAnnual;
                case "A":
                case "NACA":
                case "ANNUAL": return CompoundingStore.Annual;

                default: throw new ArgumentException(strValue + " is not a known compounding convention in input: " + inputName);
            }
        }


        public static Calendar GetCalendarFromString(string strValue, string inputName)
        {
            return StaticData.GetCalendar(strValue.ToUpper());            
        }

        public static BusinessDayConvention GetBusinessDayConventionFromString(string strValue, string inputName)
        {

            switch (strValue.ToUpper())
            {
                case "F":
                case "FOLLOWING": return BusinessDayStore.Following;
                case "MF":
                case "MODFOLLOW": 
                case "MODIFIEDFOLLOWING": return BusinessDayStore.ModifiedFollowing;
                case "P":
                case "PRECEDING": return BusinessDayStore.Preceding;
                case "MP":
                case "MODIFIEDPRECEDING": return BusinessDayStore.ModifiedPreceding;
                case "U":
                case "UNADJUSTED": return BusinessDayStore.Unadjusted;
                    
                default: throw new ArgumentException(strValue + " is not a known business day convention convention in input: " + inputName);
            }
        }


        public static DayCountConvention GetDayCountConventionFromString(string strValue, string inputName)
        {
            switch (strValue.ToUpper())
            {
                case "ACTACT": return DayCountStore.ActActISDA;
                case "ACT360": return DayCountStore.Actual360;
                case "ACT365F":
                case "ACT365": return DayCountStore.Actual365Fixed;
                case "30360EU": return DayCountStore.Thirty360Euro;                

                default: throw new ArgumentException(strValue + " is not a known day count convention: " + inputName);
            }
        }


        private static FloatingIndex GetFloatingIndexFromString(string strValue, string inputName)
        {
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
                default:
                    throw new ArgumentException(strValue + " is not a known floating rate index in input: " + inputName);
            }
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
        /// Return a reference entity from a string descriptor.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static ReferenceEntity GetReferenceEntityFromString(string strValue, string inputName)
        {
            return new ReferenceEntity(strValue);
        }

        /// <summary>
        /// Used by the various GetCurrencies utility methods.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static T GetSpecialType<T>(object obj, string inputName, T defaultValue = null) where T : class
        {
            if (obj is ExcelMissing)
                if (defaultValue != null)
                    return defaultValue;
                else
                    throw new ArgumentException(inputName + " cannot be empty.");
            if (obj is string)
            {
                string strValue = (string)obj;
                if (strValue.IndexOf('.')>0) // If the string looks like a reference to an object then get that object off the map
                {
                    //TODO: Make a function that checks if a string looks like an object reference.
                    return ObjectMap.Instance.GetObjectFromID<T>(strValue);
                }
                switch (typeof(T).Name)
                {
                    case ("Currency"):
                        {
                            Currency temp = GetCurrencyFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    case ("FloatingIndex"):
                        {
                            FloatingIndex temp = GetFloatingIndexFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    case ("Tenor"):
                        {                            
                            Tenor temp = GetTenorFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    case ("ReferenceEntity"):
                        {
                            ReferenceEntity temp = GetReferenceEntityFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    case ("Share"):
                        {
                            Share temp = GetShareFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    case ("String"):
                        {
                            string temp = GetStringFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    case ("CompoundingConvention"):
                        return (T)GetCompoundingConventionFromString((string)obj, inputName);
                    case ("DayCountConvention"):
                        return (T)GetDayCountConventionFromString((string)obj, inputName);
                    case ("BusinessDayConvention"):
                        return (T)GetBusinessDayConventionFromString((string)obj, inputName);
                    case ("Calendar"):
                        {
                            Calendar temp = GetCalendarFromString((string)obj, inputName);
                            T returnVal = temp as T;
                            return returnVal;
                        }
                    default:
                        throw new ArgumentException("No conversion exists from string to " + typeof(T).Name);                        
                }
            }
            else
                throw new ArgumentException(inputName + " : " + typeof(T).Name + " can only be created from a string.");
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
        public static T GetSpecialType0D<T>(object[,] values, string inputName, T defaultValue = null)  where T : class
        {            
            if (values.GetLength(0) == 1 && values.GetLength(1) == 1)
            {
                return GetSpecialType<T>(values[0, 0], inputName, defaultValue);
            }
            throw new ArgumentException(inputName + " must be a single cell with a string representing a " + typeof(T).Name + ".");
        }


        /// <summary>
        /// Get an array of <typeparamref name="T"/> from an excel range of strings.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static T[] GetSpecialType1D<T>(object[,] values, string inputName) where T : class
        {
            if (values.GetLength(0) == 1 && values.GetLength(1) >= 1) // row of inputs
            {
                T[] result = new T[values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                    result[i] = GetSpecialType<T>(values[0, i], inputName, null);
                return result;
            }
            else if (values.GetLength(0) >= 1 && values.GetLength(1) == 1) // column of inputs
            {
                T[] result = new T[values.GetLength(0)];
                for (int i = 0; i < values.GetLength(0); i++)
                    result[i] = GetSpecialType<T>(values[i, 0], inputName, null);
                return result;
            }
            else
                throw new ArgumentException(inputName + " must be a single row or column of strings, each representing a " + typeof(T).Name + ".");
        }

        /// <summary>
        /// Gets an 2D array of <typeparamref name="T"/> from an excel range of strings.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="inputName">The name of the input in the Excel function so that sensible errors can be returned.</param>
        /// <returns></returns>
        public static T[,] GetSpecialType2D<T>(object[,] values, string inputName) where T : class
        {
            T[,] result = new T[values.GetLength(0), values.GetLength(1)];
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    result[i, j] = GetSpecialType<T>(values[0, i], inputName, null);
                }
            }
            return result;
        }


        /// <summary>
        /// Gets an integer value from an Excel value.  The input value needs to be a number and will be rounded to an int.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
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
