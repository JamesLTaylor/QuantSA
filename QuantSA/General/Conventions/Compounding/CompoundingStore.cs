using System;

namespace QuantSA.General.Conventions.Compounding
{
    /// <summary>
    /// A collection of the compounding conventions available in QuantSA.  They are all singleton instances.
    /// </summary>
    public static class CompoundingStore
    {
        public static Simple Simple = Simple.Instance;
        public static Discount Discount = Discount.Instance;
        public static Periodically Daily = Periodically.DailyInstance;
        public static Periodically Monthly = Periodically.MonthlyInstance;
        public static Periodically Quarterly = Periodically.QuarterlyInstance;
        public static Periodically SemiAnnual = Periodically.SemiAnnualInstance;
        public static Periodically Annual = Periodically.AnnualInstance;
        public static Continuous Continuous = Continuous.Instance;
    }

    public class Simple : ICompounding
    {
        public static readonly Simple Instance = new Simple();
        private Simple() { }
        public double DF(double rate, double yearFraction)
        {
            return 1.0 / (1 + rate * yearFraction);
        }
    }

    public class Discount : ICompounding
    {
        public static readonly Discount Instance = new Discount();
        private Discount() { }
        public double DF(double rate, double yearFraction)
        {
            return (1 - rate * yearFraction);
        }
    }

    public class Periodically : ICompounding
    {
        private int n;
        public static readonly Periodically DailyInstance = new Periodically(365);
        public static readonly Periodically MonthlyInstance = new Periodically(12);
        public static readonly Periodically QuarterlyInstance = new Periodically(4);
        public static readonly Periodically SemiAnnualInstance = new Periodically(2);
        public static readonly Periodically AnnualInstance = new Periodically(1);

        private Periodically(int n) { this.n = n; }
        public double DF(double rate, double yearFraction)
        {
            return Math.Pow(1 + rate / 365, 365 * yearFraction);
        }
    }

    public class Continuous : ICompounding
    {
        private static readonly Continuous instance = new Continuous();
        private Continuous() { }
        public static Continuous Instance
        { get { return instance; } }
        public double DF(double rate, double yearFraction)
        {
            return Math.Exp(-rate * yearFraction);
        }
    }


}
