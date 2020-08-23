using System;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Shared.Exceptions
{
    /// <summary>
    /// Thrown when a particular operation is attempted on a <see cref="MarketObservable"/> for which it
    /// is not defined.
    /// </summary>
    public class MarketObservableNotSupportedException : Exception
    {
        public MarketObservableNotSupportedException(string message) : base(message)
        {
        }
    }
}