namespace QuantSA.General.Conventions.Compounding
{
    /// <summary>
    /// A compounding convention defines how a rate and year fraction are converted to a 
    /// discount or accrual factor.
    /// </summary>
    public interface ICompounding
    {
        double DF(double rate, double yearFraction);
    }
}
