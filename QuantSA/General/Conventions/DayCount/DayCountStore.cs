using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General.Conventions.DayCount
{
    /// <summary>
    /// A collection of the daycounts available in QuantSA.  Some that require arguments in their 
    /// </summary>
    public static class DayCountStore
    {
        public static Actual365Fixed Actual365Fixed = Actual365Fixed.Instance;
        public static ActActISDA ActActISDA = ActActISDA.Instance;
    }
}
