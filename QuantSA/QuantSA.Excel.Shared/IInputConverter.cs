using System;

namespace QuantSA.Excel.Shared
{
    /// <summary>
    /// A class that converts an object coming from excel to one of
    /// type <see cref="RequiredType"/>.
    /// </summary>
    public interface IInputConverter
    {
        Type RequiredType { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">If this is not populated in the Excel function it will come through as null.</param>
        /// <param name="inputName">The name of the argument to be used in giving sensible error messages.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        object Convert(object input, string inputName, string defaultValue);
    }
}
