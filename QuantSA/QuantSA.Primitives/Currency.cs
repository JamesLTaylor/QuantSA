using System;

namespace QuantSA.Primitives
{
    [Serializable]
    public class Currency
    {
        private string code;

        private Currency(string code)
        { this.code = code.ToUpper(); }

        /// <summary>
        /// Make sure that the string value will be unique for each unique instance.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        { return code; }


        public sealed override bool Equals(object obj)
        {
            Currency right = obj as Currency;
            if (right == null) return false;
            return this == right;
        }

        public static bool operator ==(Currency left, Currency right)
        {
            if ((object)left == null && (object)right == null) return true;
            if ((object)left != null && (object)right == null) return false;
            if ((object)left == null && (object)right != null) return false;
            if (left.ToString() == "ANY" || right.ToString() == "ANY") return true;
            return (left.GetHashCode() == right.GetHashCode());
        }

        public static bool operator !=(Currency left, Currency right)
        {
            return !(left == right);
        }

        public sealed override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #region Stored Currencies
        public static Currency ZAR = new Currency("ZAR");
        public static Currency USD = new Currency("USD");
        public static Currency EUR = new Currency("EUR");
        /// <summary>
        /// A special currency that will return true when compared with any other currency.  Allows one to quickly make a curve 
        /// that can be used for any product.
        /// </summary>
        public static Currency ANY = new Currency("ANY");
        #endregion

    }
}
