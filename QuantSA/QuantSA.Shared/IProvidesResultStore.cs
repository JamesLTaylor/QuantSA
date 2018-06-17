using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Shared;

namespace QuantSA.General
{
    public interface IProvidesResultStore
    {
        ResultStore GetResultStore();
    }
}
