using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    public interface IDiscountingSource
    {
        double GetDF(Date date);
    }
}
