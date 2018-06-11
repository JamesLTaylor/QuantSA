using QuantSA.Shared.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public class Actual360 : DayCountConvention
    {
        public static readonly Actual360 Instance = new Actual360();

        private Actual360()
        {
        }

        public double YearFraction(Date date1, Date date2)
        {
            return (date2 - date1) / 360.0;
        }
    }
}