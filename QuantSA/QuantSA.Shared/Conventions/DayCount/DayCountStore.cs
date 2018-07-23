using QuantSA.General.Conventions.DayCount;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Conventions.DayCount
{
    /// <summary>
    /// A collection of the daycounts available in QuantSA.  The ones that require no arguments to 
    /// their constructors are singleton instances.
    /// </summary>
    public static class DayCountStore
    {
        public static Actual365Fixed Actual365Fixed = Actual365Fixed.Instance;
        public static Actual360 Actual360 = Actual360.Instance;
        public static ActActISDA ActActISDA = ActActISDA.Instance;
        public static Thirty360Euro Thirty360Euro = Thirty360Euro.Instance;

        public static Business252 Business252(Calendar calendar)
        {
            return new Business252(calendar);
        }

        //TODO: Ensure a singleton instance of Business252.
    }
}