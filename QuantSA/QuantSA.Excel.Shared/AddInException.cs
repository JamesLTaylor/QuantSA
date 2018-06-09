using System;

namespace QuantSA.Excel.Shared
{
    /// <summary>
    /// Represents any error where the QuantSA AddIn or its plugins are not able to be loaded properly.
    /// </summary>
    public class AddInException : Exception
    {
        public AddInException(string message) : base(message)
        {
        }
    }
}