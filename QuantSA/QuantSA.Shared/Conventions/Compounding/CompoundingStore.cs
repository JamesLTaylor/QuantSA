using System;

namespace QuantSA.Shared.Conventions.Compounding
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

    public class Simple : ICompoundingConvention
    {
        public static readonly Simple Instance = new Simple();

        private Simple()
        {
        }

        public double DfFromRate(double rate, double yearFraction)
        {
            return 1.0 / (1 + rate * yearFraction);
        }

        public double RateFromDf(double df, double yearFraction)
        {
            return (1 / df - 1) / yearFraction;
        }
    }

    public class Discount : ICompoundingConvention
    {
        public static readonly Discount Instance = new Discount();

        private Discount()
        {
        }

        public double DfFromRate(double rate, double yearFraction)
        {
            return 1 - rate * yearFraction;
        }

        public double RateFromDf(double df, double yearFraction)
        {
            return (1 - df) / yearFraction;
        }
    }

    public class Periodically : ICompoundingConvention
    {
        public static readonly Periodically DailyInstance = new Periodically(365);
        public static readonly Periodically MonthlyInstance = new Periodically(12);
        public static readonly Periodically QuarterlyInstance = new Periodically(4);
        public static readonly Periodically SemiAnnualInstance = new Periodically(2);
        public static readonly Periodically AnnualInstance = new Periodically(1);
        private readonly int _n;

        private Periodically(int n)
        {
            _n = n;
        }

        public double DfFromRate(double rate, double yearFraction)
        {
            return Math.Pow(1 + rate / _n, -_n * yearFraction);
        }

        public double RateFromDf(double df, double yearFraction)
        {
            return (Math.Pow(df, -1 / (_n * yearFraction)) - 1) * _n;
        }
    }

    public class Continuous : ICompoundingConvention
    {
        private Continuous()
        {
        }

        public static Continuous Instance { get; } = new Continuous();

        public double DfFromRate(double rate, double yearFraction)
        {
            return Math.Exp(-rate * yearFraction);
        }

        public double RateFromDf(double df, double yearFraction)
        {
            return -Math.Log(df) / yearFraction;
        }
    }
}