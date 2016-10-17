using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValuationTest;

namespace TestPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            EquitySimulatorTest test = new EquitySimulatorTest();
            test.Init();
            test.TestEquitySimulatorWithRateForecast();
        }
    }
}
