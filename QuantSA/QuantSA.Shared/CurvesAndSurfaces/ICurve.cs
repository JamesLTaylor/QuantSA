using QuantSA.Shared.Dates;

namespace QuantSA.Shared.CurvesAndSurfaces
{
    /// <summary>
    /// A curve is simply a function between dates and values.  
    /// </summary>
    public interface ICurve
    {
        double InterpAtDate(Date date);
    }
}