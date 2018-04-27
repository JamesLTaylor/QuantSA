using QuantSA.General.Conventions.DayCount;
using QuantSA.Primitives.Dates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Primitives.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public interface DayCountConvention
    {
        double YearFraction(Date date1, Date date2);
    }
}
    
