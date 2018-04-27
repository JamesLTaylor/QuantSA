using System;

namespace QuantSA.Primitives.MarketObservables
{
    /// <summary>
    /// An object to describe a floating rate index such as 3 Month Jibar.
    /// </summary>
    [Serializable]
    public class FloatingIndex : MarketObservable
    {
        public Currency currency { get; private set; }
        private string name;
        private string toString;
        public Tenor tenor { get; private set; }        

        private FloatingIndex(Currency currency, string name, Tenor tenor)
        {
            this.currency = currency;
            this.name = name;
            this.tenor = tenor;
            toString = currency.ToString() + ":" + name.ToUpper() + ":" + tenor.ToString();
        }

        public override string ToString()
        {
            return toString;
        }

        #region Stored Indices        
        /// When you add new indices here remember to add them to: 
        /// ExcelUtilties.GetFloatingIndices0D 
        /// 
        public static FloatingIndex JIBAR3M = new FloatingIndex(Currency.ZAR, "Jibar", Tenor.Months(3));
        public static FloatingIndex JIBAR6M = new FloatingIndex(Currency.ZAR, "Jibar", Tenor.Months(6));
        public static FloatingIndex JIBAR1M = new FloatingIndex(Currency.ZAR, "Jibar", Tenor.Months(1));
        public static FloatingIndex PRIME1M_AVG = new FloatingIndex(Currency.ZAR, "Prime1MonthAvg", Tenor.Months(1));
        public static FloatingIndex LIBOR3M = new FloatingIndex(Currency.USD, "Libor", Tenor.Months(3));
        public static FloatingIndex LIBOR6M = new FloatingIndex(Currency.USD, "Libor", Tenor.Months(6));
        public static FloatingIndex LIBOR1M = new FloatingIndex(Currency.USD, "Libor", Tenor.Months(1));
        public static FloatingIndex EURIBOR3M = new FloatingIndex(Currency.EUR, "Euribor", Tenor.Months(3));
        public static FloatingIndex EURIBOR6M = new FloatingIndex(Currency.EUR, "Euribor", Tenor.Months(6));
        #endregion
    }
}
