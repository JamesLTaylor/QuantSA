using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Conventions.DayCount
{
    public interface IDayCountConvention
    {
        double YearFraction(Date date1, Date date2);
    }
}