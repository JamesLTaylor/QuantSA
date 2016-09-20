using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General;
using ExcelDna.Integration;
using QuantSA;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Excel;

namespace QuantSA.Excel
{
    public class XLGeneral
    {
        [ExcelFunction(Description = "Adds a serialized object onto the object map.  Used by plugins to interact with the core library.",
        Name = "QSA.LatestError",
        Category = "QSA.General",
        IsMacroType = true,
        IsHidden = true)]
        public static object LatestError(string name, string serializedObject, int deserialize)
        {
            try {
                if (ExcelUtilities.latestException == null)
                {
                    LatestError latestError = new LatestError("No errors have occured.");
                    latestError.ShowDialog();

                }
                else
                {
                    LatestError latestError = new LatestError(ExcelUtilities.latestException);
                    latestError.ShowDialog();
                }
                return "";
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }
        

        [ExcelFunction(Description = "Adds a serialized object onto the object map.  Used by plugins to interact with the core library.",
        Name = "QSA.PutOnMap",
        Category = "QSA.General",
        IsHidden = true)]
        public static string PutOnMap(string name, string serializedObject, int deserialize)
        {
            try {
                if (deserialize == 1)
                {
                    byte[] bytes = Convert.FromBase64String(serializedObject);
                    MemoryStream stream = new MemoryStream(bytes);
                    object obj = new BinaryFormatter().Deserialize(stream);
                    stream.Close();
                    return ObjectMap.Instance.AddObject(name, obj);
                }
                else
                {
                    return ObjectMap.Instance.AddObject(name, serializedObject);
                }
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
        }


        [ExcelFunction(Description = "Returns a serialized version of an object from the map.  Used by plugins to interact with the core library.",
        Name = "QSA.GetFromMap",
        Category = "QSA.General",
        IsHidden = true)]
        public static string GetFromMap(string objectName, int deserialize)
        {
            try
            {
                if (deserialize == 1)
                {
                    object obj = ObjectMap.Instance.GetObjectFromID<object>(objectName);
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    new BinaryFormatter().Serialize(stream, obj);
                    string serializedObject = Convert.ToBase64String(stream.ToArray());
                    stream.Close();
                    return serializedObject;
                }
                else
                {
                    string serializedObject = ObjectMap.Instance.GetObjectFromID<string>(objectName);
                    return serializedObject;
                }
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
        }

        [QuantSAExcelFunction(Description = "Create a C# representation of data in a spreadsheet.",
        Name = "QSA.GetCSArray",
        Category = "QSA.Developer",
        IsHidden = false)]
        public static object[,] GetCSArray([ExcelArgument(Description = "The block of values you want to use in C#.")]object[,] data,
            double decimalPlaces)
        {
            try
            {
                int iDecimalPlaces = (int)decimalPlaces;
                object[,] result = new object[data.GetLength(0), 1];
                StringBuilder sb;
                for (int i = 0; i<data.GetLength(0); i++)
                {
                    sb = new StringBuilder();
                    sb.Append((i == 0) ? "{{" : "{");                    
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        if (j > 0) sb.Append(",");
                        double value = (double)data[i, j];
                        sb.Append(value.ToString($"F{iDecimalPlaces}"));                        
                    }
                    sb.Append((i == data.GetLength(0)-1) ? "}}" : "},");
                    result[i, 0] = sb.ToString();
                }
                return result;
                
            }
            catch (Exception e)
            {
                return new object[,] { { e.Message } };
            }
        }



        [QuantSAExcelFunction(Description = "Get a list of available results in the results object.",
        Name = "QSA.GetAvailableResults",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static object[,] GetAvailableResults([ExcelArgument(Description = "The name of the results object as returned by call to another QuantSA function")]string objectName)
        {
            try
            {
                IProvidesResultStore resultStore = ObjectMap.Instance.GetObjectFromID<IProvidesResultStore>(objectName);                
                string[] temp = resultStore.GetResultStore().GetNames();
                object[,] column = new object[temp.Length, 1];
                for (int i = 0; i< temp.Length; i++)
                {
                    column[i, 0] = temp[i];
                }
                return column;
            }
            catch (Exception e)
            {
                return new object[,] { { e.Message } };
            }
        }

        [QuantSAExcelFunction(Description = "Get the stored results of a calculation from a results object.",
        Name = "QSA.GetResults",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static object[,] GetResults([ExcelArgument(Description = "The name of the results object as returned by a call to another QuantSA function")]string objectName,
            [ExcelArgument(Description = "The name of the result required.  Use QSA.GetAvailableResults to get a list of all availabale results in this object.")]string resultName)
        {
            try
            {
                IProvidesResultStore resultStore = ObjectMap.Instance.GetObjectFromID<IProvidesResultStore>(objectName);
                if (resultStore == null) throw new ArgumentException("The provided object is not a results object.");
                if (resultStore.GetResultStore().IsDate(resultName))
                {
                    Date[,] dates = resultStore.GetResultStore().GetDates(resultName);
                    return ExcelUtilities.GetObjects(dates);
                }
                else if (resultStore.GetResultStore().IsString(resultName))
                {
                    return ExcelUtilities.GetObjects(resultStore.GetResultStore().GetStrings(resultName));
                }
                else
                {
                    return ExcelUtilities.GetObjects(resultStore.GetResultStore().Get(resultName));
                }
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error2D(e);                
            }
        }


        [QuantSAExcelFunction(Description = "Create a curve of dates and rates.",
        Name = "QSA.CreateDatesAndRatesCurve",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static string CreateDatesAndRatesCurve([ExcelArgument(Description = "The name of the curve.")]string name,
            [ExcelArgument(Description = "The dates at which the rates apply.")]double[] dates,
            [ExcelArgument(Description = "The rates.")]double[] rates)
        {
            try {
                var dDates = ExcelUtilities.GetDates(dates);
                DatesAndRates curve = new DatesAndRates(dDates[0], dDates, rates);
                return ObjectMap.Instance.AddObject(name, curve);
            } catch (Exception e)
            {
                return e.Message;
            }
        }




        [QuantSAExcelFunction(Description = "Get the covariance in log returns from a blob of curves.",
        Name = "QSA.CovarianceFromCurves",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static double[,] CovarianceFromCurves([ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]double[,] curves)
        {
            double[,] covMatrix = QuantSA.General.DataAnalysis.PCA.CovarianceFromCurves(curves);
            return covMatrix;
        }

        [QuantSAExcelFunction(Description = "Perform a PCA on the log returns of a blob of curves.",
        Name = "QSA.PCAFromCurves",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static double[,] PCAFromCurves([ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]double[,] curves)
        {
            double[,] covMatrix = QuantSA.General.DataAnalysis.PCA.PCAFromCurves(curves);
            return covMatrix;
        }
    }
}
