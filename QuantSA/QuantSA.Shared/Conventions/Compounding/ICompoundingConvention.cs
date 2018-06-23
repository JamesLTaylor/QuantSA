namespace QuantSA.Shared.Conventions.Compounding
{
    /// <summary>
    /// A compounding convention defines how a rate and year fraction are converted to a 
    /// discount or accrual factor.
    /// </summary>
    public interface ICompoundingConvention
    {
        double DfFromRate(double rate, double yearFraction);
        double RateFromDf(double df, double yearFraction);
    }
}
