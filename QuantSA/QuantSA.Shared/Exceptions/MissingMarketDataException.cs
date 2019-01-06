using System;

namespace QuantSA.Shared.Exceptions
{
    /// <summary>
    /// Thrown when a particular piece of required market data is not available.
    /// </summary>
    public class MissingMarketDataException : Exception
    {
        public MissingMarketDataException(string message) : base(message)
        {
        }
    }
}