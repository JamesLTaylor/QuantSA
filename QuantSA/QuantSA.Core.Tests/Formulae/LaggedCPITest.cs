using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;
using QuantSA.Core.Formulae;


namespace QuantSA.CoreExtensions.Test.Formulae
{
    [TestClass]

    public class LaggedCPITest
    {
        [TestMethod]
        public void TestLaggedCPI()
        {
            var targetDate = new Date(2005, 8, 31);

            Date[] cpiDates =
            { new Date(2005,3,1), new Date(2005,4,1), new Date(2005,5,1), new Date(2005,6,1), new Date(2005,7,1), new Date(2005,8,1),
              new Date(2005,9,1), new Date(2005,10,1), new Date(2005,11,1), new Date(2005,12,1), new Date(2006,1,1), new Date(2006,2,1),
              new Date(2006,3,1), new Date(2006,4,1), new Date(2006,5,1), new Date(2006,6,1), new Date(2006,7,1), new Date(2006,8,1),
              new Date(2006,9,1), new Date(2006,10,1), new Date(2006,11,1), new Date(2006,12,1), new Date(2007,1,1), new Date(2007,2,1),
              new Date(2007,3,1), new Date(2007,4,1), new Date(2007,5,1), new Date(2007,6,1), new Date(2007,7,1), new Date(2007,8,1),
              new Date(2007,9,1),new Date(2007,10,1)
,             };

            double[] cpiRates =
            {
                126.90, 127.60, 127.60, 127.40, 128.50, 129.0, 129.50, 129.60, 129.50, 129.50, 130.40, 130.50, 131.20, 131.80, 132.60, 133.60, 134.90,
                136.0, 136.30, 136.60, 136.50, 137.0, 138.20, 139.20, 138.0, 141.0, 141.80, 143.0, 144.40, 145.10, 146.10, 147.40
            };

            var cpiTargetDate = LaggedCPI.GetCPI(targetDate, cpiDates, cpiRates);

            Assert.AreEqual(127.60, (double)cpiTargetDate);
        }
    }
}
