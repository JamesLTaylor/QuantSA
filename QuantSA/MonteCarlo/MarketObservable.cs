namespace MonteCarlo
{
    public abstract class MarketObservable
    {
        /// <summary>
        /// This needs to be implemented very carefully becasue it is used for generating hashcodes and 
        /// testing equality.  Make sure that the string value will be unique for each unique instance.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        public sealed override bool Equals(object obj) {
            return ToString().Equals(obj.ToString());
        }        

        public sealed override int GetHashCode()
        {
            return ToString().GetHashCode();            
        }
    }
}