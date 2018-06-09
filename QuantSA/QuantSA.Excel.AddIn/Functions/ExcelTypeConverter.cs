using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;
using QuantSA.Primitives.Dates;

namespace QuantSA.Excel.Addin.Functions
{
    /// <summary>
    /// The top level input and output converter.  It is populated via reflection over the
    /// QuantSA assemblies to find all the specific type converters that implement
    /// <see cref="IInputConverter"/> and <see cref="IOutputConverter"/>.
    /// </summary>
    public static class ExcelTypeConverter
    {
        private static readonly Dictionary<Type, IInputConverter> InputConverters =
            new Dictionary<Type, IInputConverter>();

        private static readonly Dictionary<Type, IOutputConverter> OutputConverters =
            new Dictionary<Type, IOutputConverter>();

        public static void AddConvertersFrom(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IInputConverter).IsAssignableFrom(type))
                {
                    var converter = Activator.CreateInstance(type) as IInputConverter;
                    InputConverters[converter.RequiredType] = converter;
                }

                if (typeof(IOutputConverter).IsAssignableFrom(type))
                {
                    var converter = Activator.CreateInstance(type) as IOutputConverter;
                    OutputConverters[converter.SuppliedType] = converter;
                }
            }
        }

        /// <summary>
        /// Does a converter for inputs of this type exist?
        /// </summary>
        /// <param name="requiredType"></param>
        /// <returns></returns>
        public static bool ShouldUseReference(Type requiredType)
        {
            // Get the type from inside the array or matrix if applicable
            var typeToCheck = requiredType;
            if (requiredType.IsArray)
                typeToCheck = requiredType.GetElementType();
            else if (typeof(IEnumerable).IsAssignableFrom(requiredType)
                     && !typeof(string).IsAssignableFrom(requiredType))
                typeToCheck = requiredType.GetGenericArguments()[0];
            if (typeToCheck == null)
                throw new ArgumentException(
                    $"{requiredType.FullName} seems to be an array or enumerable but I can not find the element type.");

            if (OutputConverters.ContainsKey(typeToCheck))
                return false;
            if (typeof(string).IsAssignableFrom(typeToCheck))
                return false;
            if (typeToCheck.IsPrimitive)
                return false;
            if (IsNullablePrimitive(typeToCheck))
                return false;

            if (typeToCheck == typeof(object))
                return false;
            return true;
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
            var newList = (IList) Activator.CreateInstance(genericListType);
            for (var i = 0; i < input.GetLength(0); i++)
                newList.Add(ConvertInputScalar(elementType, input[i, 0], inputName, defaultValue));
            return newList;
        }

        private static object ConvertInputMatrix(Type requiredType, object[,] input, string inputName,
            string defaultValue)
        {
            var elementType = requiredType.GetElementType();
            var size = new[] {input.GetLength(0), input.GetLength(1)};
            var matrix = Array.CreateInstance(elementType, size);
            for (var i = 0; i < input.GetLength(0); i++)
            for (var j = 0; j < input.GetLength(1); j++)
                matrix.SetValue(ConvertInputScalar(elementType, input[i, j], inputName, defaultValue), new[] {i, j});
            return matrix;
        }

        private static object ConvertInputArray1D(Type requiredType, object[,] input, string inputName,
            string defaultValue)
        {
            if (input.GetLength(0) == 1 && input.GetLength(1) == 1 &&
                (input[0, 0] is ExcelMissing || input[0, 0] is ExcelEmpty))
            {
                if (defaultValue != string.Empty)
                    return defaultValue;
                throw new ArgumentException($"{inputName} is not an optional input.  Please provide a value.");
            }

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

        /// <summary>
        /// All input conversions ultimately come through this point.
        /// </summary>
        /// <param name="requiredType"></param>
        /// <param name="input"></param>
        /// <param name="inputName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static object ConvertInputScalar(Type requiredType, object input, string inputName,
            string defaultValue)
        {
            if (input is string objName && ObjectMap.Instance.TryGetObjectFromID(objName, out var element))
                return element;

            if (input is ExcelMissing || input is ExcelEmpty) input = null;
            if (IsNullablePrimitive(requiredType))
            {
                if (input != null)
                    return GetPrimitive(requiredType, input);
                if (defaultValue == string.Empty || defaultValue == string.Empty) return null;
                return GetPrimitiveFromDefault(Nullable.GetUnderlyingType(requiredType), defaultValue);
            }

            if (requiredType.IsPrimitive)
            {
                if (input != null)
                    return GetPrimitive(requiredType, input);
                if (defaultValue == string.Empty)
                    throw new ArgumentException($"{inputName} is not an optional input.  Please provide a value.");
                if (defaultValue == null)
                    throw new ArgumentException(
                        $"{inputName} is not a nullable type.  Default can not be null."); // TODO: JT: This should be checked in a unit test
                return GetPrimitiveFromDefault(requiredType, defaultValue);
            }

            if (requiredType.IsAssignableFrom(typeof(string)))
                return input ?? defaultValue;
            if (InputConverters.ContainsKey(requiredType))
                return InputConverters[requiredType].Convert(input, inputName, defaultValue);

            if (defaultValue != string.Empty) return defaultValue;

            if (input is string objNameAgain)
                throw new ArgumentException(
                    $"{inputName}: No converter for type: {requiredType.Name} and no object named {objNameAgain} on the map.");
            throw new ArgumentException($"{inputName}: No converter for type: {requiredType.Name}.");
        }

        private static object GetPrimitive(Type requiredType, object input)
        {
            if (requiredType.IsAssignableFrom(typeof(int)))
                return (int) Math.Round((double) input);
            return input;
        }

        private static object GetPrimitiveFromDefault(Type requiredType, string defaultValue)
        {
            if (defaultValue == null) return null;
            if (requiredType.IsAssignableFrom(typeof(bool)))
                return bool.Parse(defaultValue);
            if (requiredType.IsAssignableFrom(typeof(double)))
                return double.Parse(defaultValue);
            if (requiredType.IsAssignableFrom(typeof(int)))
                return int.Parse(defaultValue);
            if (requiredType.IsAssignableFrom(typeof(string)))
                return defaultValue;
            throw new ArgumentException($"Unable to create a type: {requiredType.Name} from {defaultValue}");
        }

        private static bool IsNullablePrimitive(Type requiredType)
        {
            return requiredType.IsGenericType
                   && requiredType.GetGenericTypeDefinition() == typeof(Nullable<>)
                   && requiredType.GetGenericArguments().Any(t => t.IsPrimitive);
        }

        public static object[,] ConvertOuput(Type suppliedType, object output, string outputName)
        {
            if (suppliedType.IsArray && suppliedType.GetArrayRank() == 1)
                return ConvertOutputArray1D(suppliedType, output, outputName);
            if (suppliedType.IsArray && suppliedType.GetArrayRank() == 2)
                return ConvertOutputMatrix(suppliedType, output, outputName);
            if (typeof(IEnumerable).IsAssignableFrom(suppliedType) && !typeof(string).IsAssignableFrom(suppliedType))
                return ConverOutputIEnumerable(suppliedType, output);
            return ConvertOutputScalarTo2D(suppliedType, output, outputName);
        }

        private static object ConvertOutputScalar(Type suppliedType, object output, string outputName)
        {
            if (ShouldUseReference(suppliedType))
                return ObjectMap.Instance.AddObject(outputName, output);
            if (suppliedType.IsPrimitive || typeof(string).IsAssignableFrom(suppliedType)) return output;
            if (OutputConverters.ContainsKey(suppliedType))
                return OutputConverters[suppliedType].Convert(output);
            if (suppliedType == typeof(object))
            {
                var outputDate = output as Date;
                if (outputDate != null)
                    return outputDate.ToOADate();
                return output;
            }

            throw new ArgumentException($"No converter for type: {suppliedType.Name}.");
        }

        private static object[,] ConvertOutputScalarTo2D(Type suppliedType, object output, string outputName)
        {
            var result = new object[1, 1];
            result[0, 0] = ConvertOutputScalar(suppliedType, output, outputName);
            return result;
        }

        private static object[,] ConverOutputIEnumerable(Type suppliedType, object output)
        {
            throw new NotImplementedException();
        }

        private static object[,] ConvertOutputMatrix(Type suppliedType, object output, string outputName)
        {
            var elementType = suppliedType.GetElementType();
            var arr2D = output as Array;
            var output2D = new object[arr2D.GetLength(0), arr2D.GetLength(1)];
            for (var i = 0; i < arr2D.GetLength(0); i++)
            for (var j = 0; j < arr2D.GetLength(1); j++)
                output2D[i, j] =
                    ConvertOutputScalar(elementType, arr2D.GetValue(new[] {i, j}), $"{outputName}_{i}_{j}");
            return output2D;
        }

        private static object[,] ConvertOutputArray1D(Type suppliedType, object output, string outputName)
        {
            var elementType = suppliedType.GetElementType();
            var arr1D = output as Array;
            var output2D = new object[arr1D.Length, 1];
            for (var i = 0; i < arr1D.Length; i++)
                output2D[i, 0] = ConvertOutputScalar(elementType, arr1D.GetValue(i), outputName + "_" + i);
            return output2D;
        }
    }
}