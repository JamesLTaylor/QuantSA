using QuantSA.Shared.Serialization;

namespace QuantSA.Shared.Primitives
{
    public class Currency : SerializableViaName
    {
        private readonly string _code;

        public Currency()
        {
        }

        public Currency(string code)
        {
            _code = code.ToUpper();
        }

        /// <summary>
        /// Make sure that the string value will be unique for each unique instance.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return _code;
        }


        public sealed override bool Equals(object obj)
        {
            var right = obj as Currency;
            if (right == null) return false;
            return this == right;
        }

        public static bool operator ==(Currency left, Currency right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left != null && (object) right == null) return false;
            if ((object) left == null) return false;
            if (left.ToString() == "ANY" || right.ToString() == "ANY") return true;
            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(Currency left, Currency right)
        {
            return !(left == right);
        }

        public sealed override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string GetName()
        {
            return _code;
        }
    }
}