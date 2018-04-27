using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    public interface IProvidesResultStore
    {
        ResultStore GetResultStore();
    }
}
