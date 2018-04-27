using System;

namespace QuantSA.Primitives.Conventions.Compounding
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

    public class Simple : CompoundingConvention
    {
        public static readonly Simple Instance = new Simple();
        private Simple() { }
        public double DF(double rate, double yearFraction)
        {
            return 1.0 / (1 + rate * yearFraction);
        }
        public double rateFromDF(double df, double yearFraction)
        {
            return (1 / df) - 1 / yearFraction;
        }        
    }

    public class Discount : CompoundingConvention
    {
        public static readonly Discount Instance = new Discount();
        private Discount() { }
        public double DF(double rate, double yearFraction)
        {
            return (1 - rate * yearFraction);
        }

        public double rateFromDF(double df, double yearFraction)
        {
            return (1 - df) / yearFraction;
        }
    }

    public class Periodically : CompoundingConvention
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
            return Math.Pow(1 + rate / n, n * yearFraction);
        }

        public double rateFromDF(double df, double yearFraction)
        {
            return (Math.Pow(df, 1 / (n * yearFraction)) - 1) * n;
        }
    }

    public class Continuous : CompoundingConvention
    {
        private static readonly Continuous instance = new Continuous();
        private Continuous() { }
        public static Continuous Instance
        { get { return instance; } }
        public double DF(double rate, double yearFraction)
        {
            return Math.Exp(-rate * yearFraction);
        }

        public double rateFromDF(double df, double yearFraction)
        {
            return (-Math.Log(df)) / yearFraction;
        }
    }


}
