using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.Conventions.DayCount
{
    public interface DayCountConvention
    {
        double YearFraction(Date date1, Date date2);
    }
}
    
