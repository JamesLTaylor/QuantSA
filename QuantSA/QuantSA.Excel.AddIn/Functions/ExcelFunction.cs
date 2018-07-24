using System;
using System.Collections.Generic;
using System.Reflection;
using ExcelDna.Integration;

namespace QuantSA.Excel.Addin.Functions
{
    /// <summary>
    /// A general Excel function that wraps QuantSA functions and handles the conversion of input
    /// and output types to Excel primitives.
    /// </summary>
    public class ExcelFunction
    {
        private readonly int _argOffset;
        private readonly List<string> _defaultValues;
        private readonly MethodInfo _methodInfo;

        public ExcelFunction(MethodInfo methodInfo, List<string> defaultValues, bool putOnMap)
        {
            _argOffset = putOnMap ? 1 : 0;
            if (defaultValues.Count != methodInfo.GetParameters().Length)
                throw new ArgumentException("defaults must have the same length.");
            _methodInfo = methodInfo;
            _defaultValues = defaultValues;
        }

        public string GetName()
        {
            return _methodInfo.Name;
        }

        public object[,] Eval(params object[] inputs)
        {
            try
            {
                var convertedInputs = new List<object>();
                for (var i = 0; i < inputs.Length - _argOffset; i++)
                {
                    var paramName = _methodInfo.GetParameters()[i].Name;
                    var inputAsMatrix = inputs[i + _argOffset] as object[,];
                    if (inputAsMatrix[0, 0] is ExcelMissing && _defaultValues[i] == string.Empty)
                        throw new ArgumentException($"{paramName}: Is left blank but is not optional.");
                    convertedInputs.Add(ExcelTypeConverter.ConvertInput(_methodInfo.GetParameters()[i].ParameterType,
                        inputAsMatrix,
                        paramName, _defaultValues[i]));
                }

                var output = _methodInfo.Invoke(null, convertedInputs.ToArray());
                var outputName = _argOffset == 0 ? null : GetOutputName(inputs[0]);
                return ExcelTypeConverter.ConvertOuput(_methodInfo.ReturnType, output, outputName);
            }
            catch (TargetInvocationException e)
            {
                var result = new object[1, 1];
                if (e.InnerException != null)
                    result[0, 0] = "ERROR: " + e.InnerException.Message;
                else
                    result[0, 0] = "ERROR: " + e.Message;
                return result;
            }
            catch (Exception e)
            {
                var result = new object[1, 1];
                result[0, 0] = "ERROR: " + e.Message;
                return result;
            }
        }

        private string GetOutputName(object input)
        {
            var inputAsMatrix = input as object[,];
            if (inputAsMatrix.GetLength(0) > 1 || inputAsMatrix.GetLength(1) > 1)
                throw new ArgumentException("Object name must be a single cell or value typed into the formula.");
            return inputAsMatrix[0, 0] as string;
        }

        public Delegate GetDelegate()
        {
            switch (_methodInfo.GetParameters().Length + _argOffset)
            {
                case 0:
                    return new XLDelegate00(() => Eval());
                case 1:
                    return new XLDelegate01(in1
                        => Eval(in1));
                case 2:
                    return new XLDelegate02((in1, in2)
                        => Eval(in1, in2));
                case 3:
                    return new XLDelegate03((in1, in2, in3)
                        => Eval(in1, in2, in3));
                case 4:
                    return new XLDelegate04((in1, in2, in3, in4)
                        => Eval(in1, in2, in3, in4));
                case 5:
                    return new XLDelegate05((in1, in2, in3, in4, in5)
                        => Eval(in1, in2, in3, in4, in5));
                case 6:
                    return new XLDelegate06((in1, in2, in3, in4, in5, in6)
                        => Eval(in1, in2, in3, in4, in5, in6));
                case 7:
                    return new XLDelegate07((in1, in2, in3, in4, in5, in6, in7)
                        => Eval(in1, in2, in3, in4, in5, in6, in7));
                case 8:
                    return new XLDelegate08((in1, in2, in3, in4, in5, in6, in7, in8)
                        => Eval(in1, in2, in3, in4, in5, in6, in7, in8));
                case 9:
                    return new XLDelegate09((in1, in2, in3, in4, in5, in6, in7, in8, in9)
                        => Eval(in1, in2, in3, in4, in5, in6, in7, in8, in9));
                case 10:
                    return new XLDelegate10((in1, in2, in3, in4, in5, in6, in7, in8, in9, in10)
                        => Eval(in1, in2, in3, in4, in5, in6, in7, in8, in9, in10));
                case 11:
                    return new XLDelegate11((in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11)
                        => Eval(in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11));
                default:
                    throw new ArgumentException("QuantSA can only handle Excel functions with up to 11 inputs.");
            }
        }

        private delegate object[,] XLDelegate00();

        private delegate object[,] XLDelegate01(object[,] arg1);

        private delegate object[,] XLDelegate02(object[,] arg1, object[,] arg2);

        private delegate object[,] XLDelegate03(object[,] arg1, object[,] arg2, object[,] arg3);

        private delegate object[,] XLDelegate04(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4);

        private delegate object[,] XLDelegate05(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5);

        private delegate object[,] XLDelegate06(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5, object[,] arg6);

        private delegate object[,] XLDelegate07(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5, object[,] arg6, object[,] arg7);

        private delegate object[,] XLDelegate08(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5, object[,] arg6, object[,] arg7, object[,] arg8);

        private delegate object[,] XLDelegate09(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5, object[,] arg6, object[,] arg7, object[,] arg8, object[,] arg9);

        private delegate object[,] XLDelegate10(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5, object[,] arg6, object[,] arg7, object[,] arg8, object[,] arg9, object[,] arg10);

        private delegate object[,] XLDelegate11(object[,] arg1, object[,] arg2, object[,] arg3, object[,] arg4,
            object[,] arg5, object[,] arg6, object[,] arg7, object[,] arg8, object[,] arg9, object[,] arg10,
            object[,] arg11);
    }
}