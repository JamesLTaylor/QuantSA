using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public class Actual365Fixed : DayCountConvention
    {
        public static readonly Actual365Fixed Instance = new Actual365Fixed();

        private Actual365Fixed() { }

        public double YearFraction(Date date1, Date date2)
        {
            return (date2 - date1 ) / 365.0;
        }
    }
}
