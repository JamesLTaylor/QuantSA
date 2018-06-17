using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public class Actual365Fixed : IDayCountConvention
    {
        public static readonly Actual365Fixed Instance = new Actual365Fixed();

        private Actual365Fixed()
        {
        }

        public double YearFraction(Date date1, Date date2)
        {
            return (date2 - date1) / 365.0;
        }
    }
}