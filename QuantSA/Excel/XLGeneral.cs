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

namespace QuantSA.Excel
{
    public class XLGeneral
    {
        [ExcelFunction(Description = "Adds a serialized object onto the object map.  Used by plugins to interact with the core library.",
        Name = "QSA.PutOnMap",
        Category = "QSA.General",
        IsHidden = true)]
        public static string PutOnMap(string name, string serializedObject)
        {
            try { 
                byte[] bytes = Convert.FromBase64String(serializedObject);
                MemoryStream stream = new MemoryStream(bytes);
                object obj = new BinaryFormatter().Deserialize(stream);
                stream.Close();
                return ObjectMap.Instance.AddObject(name, obj);
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
        public static string GetFromMap(string objectName)
        {
            try
            {
                object obj = ObjectMap.Instance.GetObjectFromID(objectName);
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                new BinaryFormatter().Serialize(stream, obj);
                string serializedObject = Convert.ToBase64String(stream.ToArray());
                stream.Close();
                return serializedObject;
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
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
                IProvidesResultStore resultStore = ObjectMap.Instance.GetObjectFromID(objectName) as IProvidesResultStore;
                if (resultStore == null) throw new ArgumentException("The provided object does not have a results object.");
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
                IProvidesResultStore resultStore = ObjectMap.Instance.GetObjectFromID(objectName) as IProvidesResultStore;
                if (resultStore == null) throw new ArgumentException("The provided object is not a results object.");
                double[,] dArray = resultStore.GetResultStore().Get(resultName);
                object[,] oArray = new object[dArray.GetLength(0), dArray.GetLength(1)];
                Array.Copy(dArray, oArray, dArray.Length);
                return oArray;
            }
            catch (Exception e)
            {
                return new object[,] { { e.Message } };
            }
        }


        [QuantSAExcelFunction(Description = "Create a curve of dates and rates",
        Name = "QSA.CreateDatesAndRatesCurve",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static string CreateDatesAndRatesCurve([ExcelArgument(Description = "The name of the curve.")]string name,
            [ExcelArgument(Description = "The dates at which the rates apply.")]double[] dates,
            [ExcelArgument(Description = "The rates.")]double[] rates)
        {
            try {
                DatesAndRates curve = new DatesAndRates(dates, rates);
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
