using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Valuation;
using System.Collections.Generic;

namespace ValuationTest
{
    [TestClass]
    public class IRSwapEPETest
    {
        [TestMethod]
        public void TestCoordinatorEPESwap()
        {
            // Make the swap
            double rate = 0.07;
            bool payFixed = true;
            double notional = 1000000;
            Date startDate = new Date(2016, 9, 17);
            Tenor tenor = Tenor.Years(5);
            IRSwap swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Set up the model
            Date valueDate = new Date(2016, 9, 17);
            double a = 0.05;
            double vol = 0.005;
            double flatCurveRate = 0.07;
            HullWhite1F hullWiteSim = new HullWhite1F(a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatingIndex.JIBAR3M);
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            Date date = valueDate;
            Date endDate = valueDate.AddTenor(tenor);
            List<Date> fwdValueDates = new List<Date>();
            while (date< endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.Days(10));
            }
            double[] epe = coordinator.EPE(new Product[] { swap }, valueDate, fwdValueDates.ToArray());
            //Debug.WriteToFile(@"c:\dev\temp\epe_rate08_vol005.csv", epe);

            Assert.AreEqual(2512.0, epe[0], 1.0);
            Assert.AreEqual(6797.2, epe[90], 34.0);
            Assert.AreEqual(1076.0, epe[182], 5.0);

        }
    }
}
