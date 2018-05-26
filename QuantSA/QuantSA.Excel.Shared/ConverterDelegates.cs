namespace QuantSA.Excel.Common
{
    public class ConverterDelegates
    {
        /// <summary>
        /// The form of a function that converts the input from excel to an object that can be cast
        /// to a known type.  Implement this when you want to have QuantSA handle the scalar, array and
        /// matrix versions of the same type.
        /// </summary>
        public delegate object InputConverter0(object input, string inputName, string defaultValue = null);

        /// <summary>
        /// The form of a function that converts the input from excel to an object that can be cast to a
        /// known type.  Implement this when you want to handle the direct object[,] from excel.
        /// </summary>
        public delegate object InputConverterFull(object[,] input, string inputName, string defaultValue = null);

        /// <summary>
        /// Converts the output from a function from a known type to an object[,] that can be rendered in
        /// Excel.
        /// </summary>
        public delegate object OutputConverter0(object output);

        /// <summary>
        /// Converts the output from a function from a known type to an object[,] that can be rendered in
        /// Excel.
        /// </summary>
        public delegate object[,] OutputConverterFull(object output);
    }
}