using QuantSA.General.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public class Business252 : DayCountConvention
    {
        private readonly Calendar calendar;
        public Business252(Calendar calendar) { this.calendar = calendar; }

        public double YearFraction(Date date1, Date date2)
        {
            int busDays = calendar.businessDaysBetween(date1, date2);            
            return busDays/252.0;
        }
    }
}
