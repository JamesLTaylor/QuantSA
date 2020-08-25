using System;

namespace QuantSA.Shared.Exceptions
{
    /// <summary>
    /// Thrown when there is something wrong with a product script.
    /// </summary>
    public class ScriptException : Exception
    {
        public ScriptException(string message) : base(message)
        {
        }
    }
}