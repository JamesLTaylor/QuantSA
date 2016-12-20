using System;
using System.Text;
using ExcelDna.Integration;
using XU = QuantSA.Excel.ExcelUtilities;
using MathNet.Numerics.Interpolation;
using QuantSA.General;

namespace QuantSA.Excel
{
    public class XLGeneral
    {
        [ExcelFunction(Description = "",
        Name = "QSA.LatestError",
        Category = "QSA.General",
        IsMacroType = true,
        IsHidden = true)]
        public static object LatestError(string name, string serializedObject, int deserialize)
        {
            try {
                if (ExcelUtilities.latestException == null)
                {
                    ExcelMessage latestError = new ExcelMessage("No errors have occured.");
                    latestError.ShowDialog();

                }
                else
                {
                    ExcelMessage latestError = new ExcelMessage(ExcelUtilities.latestException);
                    latestError.ShowDialog();
                }
                return "";
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }
        
        [QuantSAExcelFunction(Description = "Create a C# representation of data in a spreadsheet.",
        Name = "QSA.GetCSArray",
        Category = "QSA.General",
        IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetCSArray.html")]
        public static object[,] GetCSArray([ExcelArgument(Description = "The block of values you want to use in C#.")]object[,] data,
            [ExcelArgument(Description = "The number of decimal places each value must have in the string.")]double decimalPlaces)
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
            ExampleSheet = "ZARSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetAvailableResults.html")]
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
            ExampleSheet = "ZARSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetResults.html")]
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
                    return ExcelUtilities.ConvertToObjects(dates);
                }
                else if (resultStore.GetResultStore().IsString(resultName))
                {
                    return ExcelUtilities.ConvertToObjects(resultStore.GetResultStore().GetStrings(resultName));
                }
                else
                {
                    return ExcelUtilities.ConvertToObjects(resultStore.GetResultStore().Get(resultName));
                }
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error2D(e);                
            }
        }


        [QuantSAExcelFunction(Description = "The Black Scholes formula for a call.",
            IsHidden = false,
            Name = "QSA.FormulaBlackScholes",
            ExampleSheet = "EquityValuation.xlsx",
            Category = "QSA.General",
            HelpTopic = "http://www.quantsa.org/FormulaBlackScholes.html")]
        public static object FormulaBlackScholes([ExcelArgument(Description = "Strike")]object[,] strike,
            [ExcelArgument(Description = "The value date as and Excel date.")]object[,] valueDate,
            [ExcelArgument(Description = "The exercise date of the option.  Must be greater than the value date.")]object[,] exerciseDate,
            [ExcelArgument(Description = "The spot proce of the underlying at the value date.")]object[,] spotPrice,
            [ExcelArgument(Description = "Annualized volatility.")]object[,] vol,
            [ExcelArgument(Description = "Continuously compounded risk free rate.")]object[,] riskfreeRate,
            [ExcelArgument(Description = "Continuously compounded dividend yield.")]object[,] divYield)
        {
            try {
                return Formulae.BlackScholes(PutOrCall.Call, XU.GetDouble0D(strike, "strike"),
                    (XU.GetDate0D(exerciseDate, "exerciseDate") - XU.GetDate0D(valueDate, "valueDate")) / 365.0,
                    XU.GetDouble0D(spotPrice, "spotPrice"), XU.GetDouble0D(vol, "vol"),
                    XU.GetDouble0D(riskfreeRate, "riskfreeRate"), XU.GetDouble0D(divYield, "divYield"));
            } catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Description = "Create a product defined in a script file.",
        IsHidden = false,
        Name = "QSA.CreateProductFromFile",
        Category = "QSA.General",
            ExampleSheet = "ProductFromFile.xlsx",
        HelpTopic = "http://www.quantsa.org/CreateProductFromFile.html")]
        public static object CreateProductFromFile([ExcelArgument(Description = "Name of product")]string name,
    [ExcelArgument(Description = "Full path to the file.")]string filename)
        {
            try
            {
                Product runtimeProduct = RuntimeProduct.CreateFromScript(filename);
                return XU.AddObject(name, runtimeProduct);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Description = "A linear interpolator.",
            IsHidden = false,
            Name = "QSA.InterpLinear",
            ExampleSheet = "InterpLinearExample.xlsx",
            Category = "QSA.General",
            HelpTopic = "http://www.quantsa.org/InterpLinear.html")]
        public static object[,] InterpLinear([ExcelArgument(Description = "A vector of x values.  Must be in increasing order")]double[] knownX,
            [ExcelArgument(Description = "A vector of y values.  Must be the same length as knownX")]Double[] knownY,
            [ExcelArgument(Description = "x values at which interpolation is required.")]Double[,] requiredX)
        {
            LinearSpline spline = LinearSpline.InterpolateSorted(knownX, knownY);
            object[,] result = new object[requiredX.GetLength(0), requiredX.GetLength(1)];

            for (int x = 0; x < requiredX.GetLength(0); x += 1)
            {
                for (int y = 0; y < requiredX.GetLength(1); y += 1)
                {
                    result[x, y] = spline.Interpolate(requiredX[x, y]);
                }
            }
            return result;
        }
    }
}
