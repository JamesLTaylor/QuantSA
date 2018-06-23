using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.Rates;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace ValuationTest
{
    [TestClass]
    public class IRSwapEPETest
    {
        [TestMethod]
        public void TestCoordinatorEPESwap()
        {
            // Make the swap
            var rate = 0.07;
            var payFixed = true;
            double notional = 1000000;
            var startDate = new Date(2016, 9, 17);
            var tenor = Tenor.FromYears(5);
            var swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            var a = 0.05;
            var vol = 0.005;
            var flatCurveRate = 0.07;
            var hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatRateIndex.JIBAR3M);
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            var date = valueDate;
            var endDate = valueDate.AddTenor(tenor);
            var fwdValueDates = new List<Date>();
            while (date < endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.FromDays(10));
            }

            var epe = coordinator.EPE(new Product[] {swap}, valueDate, fwdValueDates.ToArray());
            //Debug.WriteToFile(@"c:\dev\temp\epe_rate08_vol005.csv", epe);

            Assert.AreEqual(2560, epe[0], 100.0);
            Assert.AreEqual(6630, epe[90], 100.0);
            Assert.AreEqual(734, epe[182], 30);
        }
    }
}