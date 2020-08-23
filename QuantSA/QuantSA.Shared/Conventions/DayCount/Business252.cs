using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Conventions.DayCount
{
    public class Business252 : IDayCountConvention
    {
        private readonly Calendar _calendar;

        public Business252(Calendar calendar)
        {
            this._calendar = calendar;
        }

        public double YearFraction(Date date1, Date date2)
        {
            var busDays = _calendar.BusinessDaysBetween(date1, date2);
            return busDays / 252.0;
        }
    }
}