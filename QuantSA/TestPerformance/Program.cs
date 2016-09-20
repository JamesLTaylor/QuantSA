using QuantSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            RateProductTest test = new RateProductTest();
            test.TestSwapHW();
        }
    }
}
