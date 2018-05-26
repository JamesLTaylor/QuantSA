using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using QuantSA.Excel.Common;

namespace QuantSA.Excel.Addin.AddIn
{
    public static class ExcelTypeConverter
    {

        private static readonly Dictionary<Type, ConverterDelegates.InputConverter0> InputConverters0 =
            new Dictionary<Type, ConverterDelegates.InputConverter0>();

        private static readonly Dictionary<Type, ConverterDelegates.InputConverterFull> InputConvertersFull =
            new Dictionary<Type, ConverterDelegates.InputConverterFull>();

        private static readonly Dictionary<Type, ConverterDelegates.OutputConverter0> OutputConverters0 =
            new Dictionary<Type, ConverterDelegates.OutputConverter0>();

        public static void AddInputConverter(Type requiredType, MethodInfo converterMethodInfo)
        {
            var converter = Delegate.CreateDelegate(typeof(ConverterDelegates.InputConverter0), converterMethodInfo) as ConverterDelegates.InputConverter0;
            InputConverters0[requiredType] = converter;
        }

        public static void AddOutputConverter(Type suppliedType, MethodInfo converterMethodInfo)
        {
            var converter = Delegate.CreateDelegate(typeof(ConverterDelegates.OutputConverter0), converterMethodInfo) as ConverterDelegates.OutputConverter0;
            OutputConverters0[suppliedType] = converter;
        }

        public static object ConvertInput(Type requiredType, object[,] input, string inputName,
            string defaultValue = null)
        {
            if (requiredType.IsArray && requiredType.GetArrayRank() == 1)
                return ConvertInputArray1D(requiredType, input, inputName, defaultValue);
            if (requiredType.IsArray && requiredType.GetArrayRank() == 2)
                return ConvertInputMatrix(requiredType, input, inputName, defaultValue);
            if (typeof(IEnumerable).IsAssignableFrom(requiredType) && !typeof(string).IsAssignableFrom(requiredType))
                return ConvertInputIEnumerable(requiredType, input, inputName, defaultValue);
            return ConvertInputScalar(requiredType, input, inputName, defaultValue);
        }

        private static object ConvertInputIEnumerable(Type requiredType, object[,] input, string inputName,
            string defaultValue)
        {
            var elementType = requiredType.GetGenericArguments()[0];
            var genericListType = typeof(List<>).MakeGenericType(elementType);
            var newList = (IList)Activator.CreateInstance(genericListType);
            for (var i = 0; i < input.GetLength(0); i++)
                newList.Add(ConvertInputScalar(elementType, input[i, 0], inputName, defaultValue));
            return newList;
        }

        private static object ConvertInputMatrix(Type requiredType, object[,] input, string inputName,
            string defaultValue)
        {
            var elementType = requiredType.GetElementType();
            var size = new[] { input.GetLength(0), input.GetLength(1) };
            var matrix = Array.CreateInstance(elementType, size);
            for (var i = 0; i < input.GetLength(0); i++)
                for (var j = 0; i < input.GetLength(1); j++)
                    matrix.SetValue(ConvertInputScalar(elementType, input[i, 0], inputName, defaultValue), new[] { i, j });
            return matrix;
        }

        private static object ConvertInputArray1D(Type requiredType, object[,] input, string inputName,
            string defaultValue)
        {
            if (input.GetLength(0) > 1 && input.GetLength(1) > 1)
                throw new ArgumentException(
                    $"{inputName}: Expected a 1 dimensional input but 2 dimensional range is being passed.");
            var elementType = requiredType.GetElementType();
            if (input.GetLength(0) > 1)
            {
                var newArray = Array.CreateInstance(elementType, input.GetLength(0));
                for (var i = 0; i < input.GetLength(0); i++)
                    newArray.SetValue(ConvertInputScalar(elementType, input[i, 0], inputName, defaultValue), i);
                return newArray;
            }
            else
            {
                var newArray = Array.CreateInstance(elementType, input.GetLength(1));
                for (var i = 0; i < input.GetLength(1); i++)
                    newArray.SetValue(ConvertInputScalar(elementType, input[0, i], inputName, defaultValue), i);
                return newArray;
            }
        }

        private static object ConvertInputScalar(Type requiredType, object[,] input, string inputName,
            string defaultValue)
        {
            if (input.GetLength(0) > 1 || input.GetLength(1) > 1)
                throw new ArgumentException($"{inputName}: expecting a scalar input but a range is being provided.");
            return ConvertInputScalar(requiredType, input[0, 0], inputName, defaultValue);
        }

        private static object ConvertInputScalar(Type requiredType, object input, string inputName,
            string defaultValue)
        {
            if (requiredType.IsPrimitive) return input;
            if (InputConverters0.ContainsKey(requiredType))
                return InputConverters0[requiredType](input, inputName, defaultValue);
            throw new ArgumentException($"{inputName}: No converter for type: {requiredType.Name}.");
        }


        public static object[,] ConvertOuput(Type suppliedType, object output)
        {
            if (suppliedType.IsArray && suppliedType.GetArrayRank() == 1)
                return ConvertOutputArray1D(suppliedType, output);
            if (suppliedType.IsArray && suppliedType.GetArrayRank() == 2)
                return ConvertOutputMatrix(suppliedType, output);
            if (typeof(IEnumerable).IsAssignableFrom(suppliedType) && !typeof(string).IsAssignableFrom(suppliedType))
                return ConverOutputIEnumerable(suppliedType, output);
            return ConvertOutputScalarTo2D(suppliedType, output);
        }

        private static object ConvertOutputScalar(Type suppliedType, object output)
        {
            if (suppliedType.IsPrimitive) return output;
            if (OutputConverters0.ContainsKey(suppliedType))
                return OutputConverters0[suppliedType](output);
            throw new ArgumentException($"No converter for type: {suppliedType.Name}.");
        }

        private static object[,] ConvertOutputScalarTo2D(Type suppliedType, object output)
        {
            var result = new object[1, 1];
            result[0, 0] = ConvertOutputScalar(suppliedType, output);
            return result;
        }

        private static object[,] ConverOutputIEnumerable(Type suppliedType, object output)
        {
            throw new NotImplementedException();
        }

        private static object[,] ConvertOutputMatrix(Type suppliedType, object output)
        {
            throw new NotImplementedException();
        }

        private static object[,] ConvertOutputArray1D(Type suppliedType, object output)
        {
            throw new NotImplementedException();
        }
    }
}