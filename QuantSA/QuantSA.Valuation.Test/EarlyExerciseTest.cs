using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Primitives.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Valuation;

namespace ValuationTest
{
    [TestClass]
    public class EarlyExerciseTest
    {
        private List<Date> exDates;
        private HullWhite1F hullWiteSim;
        private IRSwap swapPay;
        private IRSwap swapRec;
        private Date valueDate;

        [TestInitialize]
        public void Init()
        {
            // Set up the model
            valueDate = new Date(2016, 9, 17);
            var a = 0.05;
            var vol = 0.01;
            var flatCurveRate = 0.07;
            hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatRateIndex.JIBAR3M);

            // Make the underlying swap
            var rate = 0.07;
            var payFixed = true;
            double notional = 1000000;
            var startDate = new Date(2016, 9, 17);
            var tenor = Tenor.Years(5);
            swapPay = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);
            swapRec = IRSwap.CreateZARSwap(rate, !payFixed, notional, startDate, tenor);

            // Full set of exercise dates
            exDates = new List<Date>
            {
                new Date(2017, 9, 17),
                new Date(2018, 9, 17),
                new Date(2019, 9, 17),
                new Date(2020, 9, 17)
            };
        }

        [TestMethod]
        public void TestBermudanSwaptionPV()
        {
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            BermudanSwaption bermudan;
            bermudan = new BermudanSwaption(swapPay, exDates.GetRange(0, 1), true);
            var value1 = coordinator.Value(new Product[] {bermudan}, valueDate);
            bermudan = new BermudanSwaption(swapPay, exDates.GetRange(0, 2), true);
            var value2 = coordinator.Value(new Product[] {bermudan}, valueDate);
            bermudan = new BermudanSwaption(swapPay, exDates.GetRange(0, 3), true);
            var value3 = coordinator.Value(new Product[] {bermudan}, valueDate);

            Assert.IsTrue(value1 < value2, "Bermudan with 1 exercise date must be worth less than one with 2.");
            Assert.IsTrue(value2 < value3, "Bermudan with 2 exercise dates must be worth less than one with 3.");
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

        [TestMethod]
        public void TestBermudanSwaptionPVLongAndShort()
        {
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            var bermudan1 = new BermudanSwaption(swapPay, exDates.GetRange(0, 1), true);
            var value1 = coordinator.Value(new Product[] {bermudan1}, valueDate);
            var bermudan2 = new BermudanSwaption(swapRec, exDates.GetRange(0, 1), false);
            var value2 = coordinator.Value(new Product[] {bermudan2}, valueDate);
            var value3 = coordinator.Value(new Product[] {bermudan1, bermudan2}, valueDate);

            //Assert.IsTrue(value1 < value2, "Bermudan with 1 exercise date must be worth less than one with 2.");
            //            Assert.IsTrue(value2 < value3, "Bermudan with 2 exercise dates must be worth less than one with 3.");
        }

        [TestMethod]
        public void TestPhysicalSwaptionEPE()
        {
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);
            var exDate = new List<Date> {new Date(2018, 9, 17)};
            // Couterparty has option to enter into a receive fixed swap
            var physicalSwaption = new BermudanSwaption(swapPay, exDate, false);

            var date = valueDate;
            var endDate = valueDate.AddTenor(new Tenor(0, 0, 3, 5));
            var fwdValueDates = new List<Date>();
            while (date <= endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.Days(10));
            }

            var epe = coordinator.EPE(new Product[] {physicalSwaption}, valueDate, fwdValueDates.ToArray());
            //Debug.WriteToFile(@"c:\dev\temp\ene_physicalswaption_HW.csv", epe);
        }
    }
}