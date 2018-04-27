using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Valuation;
using System.Collections.Generic;
using QuantSA.Primitives;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.MarketObservables;
using QuantSA.Primitives.Products;
using QuantSA.Primitives.Products.Rates;
using QuantSA.Valuation.Models.Rates;

namespace ValuationTest
{
    [TestClass]
    public class EarlyExerciseTest
    {
        Date valueDate;
        HullWhite1F hullWiteSim;
        IRSwap swapPay;
        IRSwap swapRec;
        List<Date> exDates;

        [TestInitialize]
        public void Init()
        {


            // Set up the model
            valueDate = new Date(2016, 9, 17);
            double a = 0.05;
            double vol = 0.01;
            double flatCurveRate = 0.07;
            hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatingIndex.JIBAR3M);

            // Make the underlying swap
            double rate = 0.07;
            bool payFixed = true;
            double notional = 1000000;
            Date startDate = new Date(2016, 9, 17);
            Tenor tenor = Tenor.Years(5);
            swapPay = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);
            swapRec = IRSwap.CreateZARSwap(rate, !payFixed, notional, startDate, tenor);

            // Full set of exercise dates
            exDates = new List<Date> { new Date(2017, 9, 17), new Date(2018, 9, 17),
                new Date(2019, 9, 17), new Date(2020, 9, 17) };
        }

        [TestMethod]
        public void TestBermudanSwaptionPV()
        {
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            BermudanSwaption bermudan;
            bermudan = new BermudanSwaption(swapPay, exDates.GetRange(0, 1), true);
            double value1 = coordinator.Value(new Product[] { bermudan }, valueDate);
            bermudan = new BermudanSwaption(swapPay, exDates.GetRange(0, 2), true);
            double value2 = coordinator.Value(new Product[] { bermudan }, valueDate);
            bermudan = new BermudanSwaption(swapPay, exDates.GetRange(0, 3), true);
            double value3 = coordinator.Value(new Product[] { bermudan }, valueDate);

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
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);
            
            BermudanSwaption bermudan1 = new BermudanSwaption(swapPay, exDates.GetRange(0, 1), true);
            double value1 = coordinator.Value(new Product[] { bermudan1 }, valueDate);
            BermudanSwaption bermudan2 = new BermudanSwaption(swapRec, exDates.GetRange(0, 1), false);
            double value2 = coordinator.Value(new Product[] { bermudan2 }, valueDate);
            double value3 = coordinator.Value(new Product[] { bermudan1, bermudan2 }, valueDate);

            //Assert.IsTrue(value1 < value2, "Bermudan with 1 exercise date must be worth less than one with 2.");
            //            Assert.IsTrue(value2 < value3, "Bermudan with 2 exercise dates must be worth less than one with 3.");
        }

        [TestMethod]
        public void TestPhysicalSwaptionEPE()
        {
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);
            List<Date> exDate = new List<Date> { new Date(2018, 9, 17) };
            // Couterparty has option to enter into a receive fixed swap
            BermudanSwaption physicalSwaption = new BermudanSwaption(swapPay, exDate, false);

            Date date = valueDate;
            Date endDate = valueDate.AddTenor(new Tenor(0,0,3,5));
            List<Date> fwdValueDates = new List<Date>();
            while (date <= endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.Days(10));
            }
            double[] epe = coordinator.EPE(new Product[] { physicalSwaption }, valueDate, fwdValueDates.ToArray());
            //Debug.WriteToFile(@"c:\dev\temp\ene_physicalswaption_HW.csv", epe);
        }
    }
}
