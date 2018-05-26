using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuantSA.Excel.Addin.AddIn
{
    /// <summary>
    /// A general Excel function that wraps QuantSA functions and handles the conversion of input
    /// and output types to Excel primitives.
    /// </summary>
    public class ExcelFunction
    {
        private readonly List<string> _defaultValues;
        private readonly MethodInfo _methodInfo;

        public ExcelFunction(MethodInfo methodInfo, List<string> defaultValues)
        {
            if (defaultValues.Count != methodInfo.GetParameters().Length)
                throw new Exception("defaults must have the same length.");
            _methodInfo = methodInfo;
            _defaultValues = defaultValues;
        }

        public object[,] Eval(params object[] inputs)
        {
            try
            {
                var convertedInputs = new List<object>();
                for (var i = 0; i < inputs.Length; i++)
                    convertedInputs.Add(ExcelTypeConverter.ConvertInput(_methodInfo.GetParameters()[i].ParameterType,
                        inputs[i] as object[,],
                        _methodInfo.GetParameters()[i].Name, _defaultValues[i]));
                var output = _methodInfo.Invoke(null, convertedInputs.ToArray());
                return ExcelTypeConverter.ConvertOuput(_methodInfo.ReturnType, output);
            }
            catch (Exception e)
            {
                var result = new object[1, 1];
                result[0, 0] = "ERROR: " + e.Message;
                return result;
            }

        }

        public Delegate GetDelegate()
        {
            switch (_methodInfo.GetParameters().Length)
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