namespace QuantSA.Shared.MarketObservables
{
    public abstract class MarketObservable
    {
        /// <summary>
        /// This needs to be implemented very carefully because it is used for generating hashcodes and 
        /// testing equality.  Make sure that the string value will be unique for each unique instance.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        public sealed override bool Equals(object obj)
        {
            return ToString().Equals(obj.ToString());
        }

        public static bool operator ==(MarketObservable left, MarketObservable right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left != null && (object) right == null) return false;
            if ((object) left == null && (object) right != null) return false;
            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(MarketObservable left, MarketObservable right)
        {
            return !(left == right);
        }

        public sealed override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}