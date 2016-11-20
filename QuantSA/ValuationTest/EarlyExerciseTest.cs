using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Valuation;
using System.Collections.Generic;

namespace ValuationTest
{
    [TestClass]
    public class EarlyExerciseTest
    {
        [TestMethod]
        public void TestBermudanSwaptionPV()
        {
            // Make the swap
            // underlying swap
            double rate = 0.07;
            bool payFixed = true;
            double notional = 1000000;
            Date startDate = new Date(2016, 9, 17);
            Tenor tenor = Tenor.Years(5);
            IRSwap swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);
            List<Date> exDates = new List<Date> { new Date(2017, 9, 17), new Date(2017, 9, 17) };
            BermudanSwaption bermudan = new BermudanSwaption(swap, exDates);

            // Set up the model
            Date valueDate = new Date(2016, 9, 17);
            double a = 0.05;
            double vol = 0.005;
            double flatCurveRate = 0.07;
            HullWhite1F hullWiteSim = new HullWhite1F(a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatingIndex.JIBAR3M);
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            Date date = valueDate;
            coordinator.ValueWithEarlyEx(new Product[] { bermudan }, valueDate);

            /*
            Date endDate = valueDate.AddTenor(tenor);
            List<Date> fwdValueDates = new List<Date>();
            while (date < endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.Days(10));
            }
            double[] epe = coordinator.EPE(new Product[] { swap }, valueDate, fwdValueDates.ToArray());
            */
        }
    }
}
