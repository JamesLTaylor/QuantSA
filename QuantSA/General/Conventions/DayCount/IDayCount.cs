using QuantSA.General.Conventions.DayCount;
using QuantSA.General.Dates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General.Conventions.DayCount
{
    public interface IDayCount
    {
        double YearFraction(Date date1, Date date2);
    }
}
    
