using QuantSA.Primitives.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public interface DayCountConvention
    {
        double YearFraction(Date date1, Date date2);
    }
}