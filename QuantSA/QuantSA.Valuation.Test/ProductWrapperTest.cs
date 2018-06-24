using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace ValuationTest
{
    [Serializable]
    public class ProductWrapperEquitySample1 : ProductWrapper
    {
        private readonly Share aaa = new Share("AAA", Currency.ZAR);
        private readonly Share alsi = new Share("ALSI", Currency.ZAR);
        private readonly Date date1 = new Date(2016, 9, 30); // The issue date of the scheme
        private readonly Date date2 = new Date(2017, 9, 30); // The first performance measurment date

        private readonly Date
            date3 = new Date(2018, 9, 30); // The second performance measurement date and the payment date.

        private readonly double threshAbs = 0.10; // AAA share must return at least 10% each year 
        private readonly double threshRel = 0.03; // AA share must outperform the ALSI by at least 3% in each year

        /// <summary>
        /// When the product is created it must tell the model which share prices it needs and
        /// on what dates it needs them.
        /// </summary>
        public ProductWrapperEquitySample1() : base(Currency.ZAR)
        {
            SetRequired(aaa, date1);
            SetRequired(aaa, date2);
            SetRequired(aaa, date3);
            SetRequired(alsi, date1);
            SetRequired(alsi, date2);
            SetRequired(alsi, date3);
            // cashflow dates
            var cfDates = new List<Date>();
            cfDates.Add(date3);
            SetCashflowDates(cfDates);
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            double w1;
            double w2;
            var year1AAAReturn = Get(aaa, date2) / Get(aaa, date1) - 1;
            var year2AAAReturn = Get(aaa, date3) / Get(aaa, date2) - 1;
            var year1ALSIReturn = Get(alsi, date2) / Get(alsi, date1) - 1;
            var year2ALSIReturn = Get(alsi, date3) / Get(alsi, date2) - 1;
            if (year1AAAReturn > threshAbs && year2AAAReturn > threshAbs)
                w1 = 1.0;
            else
                w1 = 0.0;

            if (year1AAAReturn - year1ALSIReturn > threshRel && year2AAAReturn - year2ALSIReturn > threshRel)
                w2 = 1.0;
            else
                w2 = 0.0;

            return new List<Cashflow> {new Cashflow(date3, Get(aaa, date3) * (w1 + w2), Currency.ZAR)};
        }
    }

    [Serializable]
    public class ProductWrapperEquitySample2 : ProductWrapper
    {
        private readonly Share aaa = new Share("AAA", Currency.ZAR);
        private readonly Share alsi = new Share("ALSI", Currency.ZAR);
        private readonly Date date1 = new Date(2016, 9, 30); // The issue date of the scheme
        private readonly Date date2 = new Date(2017, 9, 30); // The first performance measurment date

        private readonly Date
            date3 = new Date(2018, 9, 30); // The second performance measurement date and the payment date.

        private readonly double threshAbs = 0.10; // AAA share must return at least 10% each year 
        private readonly double threshRel = 0.03; // AA share must outperform the ALSI by at least 3% in each year

        public ProductWrapperEquitySample2()
        {
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            double w1;
            double w2;
            var year1AAAReturn = Get(aaa, date2) / Get(aaa, date1) - 1;
            var year2AAAReturn = Get(aaa, date3) / Get(aaa, date2) - 1;
            var year1ALSIReturn = Get(alsi, date2) / Get(alsi, date1) - 1;
            var year2ALSIReturn = Get(alsi, date3) / Get(alsi, date2) - 1;
            if (year1AAAReturn > threshAbs && year2AAAReturn > threshAbs)
                w1 = 1.0;
            else
                w1 = 0.0;

            if (year1AAAReturn - year1ALSIReturn > threshRel && year2AAAReturn - year2ALSIReturn > threshRel)
                w2 = 1.0;
            else
                w2 = 0.0;

            return new List<Cashflow> {new Cashflow(date3, Get(aaa, date3) * (w1 + w2), Currency.ZAR)};
        }
    }

    [TestClass]
    public class ProductWrapperTest
    {
        [TestMethod]
        public void TestProductWrapperEquityValuation()
        {
            Product product1 = new ProductWrapperEquitySample1();
            Product product2 = new ProductWrapperEquitySample2();

            // The model
            var anchorDate = new Date(2016, 09, 30);
            var shares = new [] {new Share("ALSI", Currency.ZAR), new Share("AAA", Currency.ZAR), new Share("BBB", Currency.ZAR)};
            double[] prices = {200, 50, 100};
            double[] vols = {0.22, 0.52, 0.4};
            double[] divYields = {0.03, 0.0, 0.0};
            double[,] correlations =
            {
                {1.0, 0.5, 0.5},
                {0.5, 1.0, 0.5},
                {0.5, 0.5, 1.0}
            };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, anchorDate,
                new[] {anchorDate, anchorDate.AddMonths(36)},
                new[] {0.07, 0.07});
            var sim = new EquitySimulator(shares, prices, vols, divYields, correlations, discountCurve,
                new IFloatingRateSource[0]);
            var coordinator = new Coordinator(sim, new List<Simulator>(), 40000);

            //Valuation
            var value1 = coordinator.Value(new[] {product1}, anchorDate);
            var value2 = coordinator.Value(new[] {product2}, anchorDate);

            Assert.AreEqual(value1, value2, value1 * 0.05);
        }
    }
}