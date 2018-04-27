namespace QuantSA.Primitives.Conventions.Compounding
{
    /// <summary>
    /// A compounding convention defines how a rate and year fraction are converted to a 
    /// discount or accrual factor.
    /// </summary>
    public interface CompoundingConvention
    {
        double DF(double rate, double yearFraction);
        double rateFromDF(double df, double yearFraction);
    }
}
