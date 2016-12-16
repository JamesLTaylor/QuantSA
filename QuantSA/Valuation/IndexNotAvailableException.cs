using System;
using System.Runtime.Serialization;

namespace QuantSA.Valuation
{
    [Serializable]
    internal class IndexNotAvailableException : Exception
    {
        public IndexNotAvailableException()
        {
        }

        public IndexNotAvailableException(string message) : base(message)
        {
        }

        public IndexNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IndexNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}