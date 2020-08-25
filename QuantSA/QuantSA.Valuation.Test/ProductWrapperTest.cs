using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Solution.Test;
using QuantSA.Valuation.Models.Equity;

namespace QuantSA.Valuation.Test
{
    public class ProductWrapperEquitySample1 : ProductWrapper
    {
        private readonly Share _aaa = new Share("AAA", TestHelpers.ZAR);
        private readonly Share _alsi = new Share("ALSI", TestHelpers.ZAR);
        private readonly Date _date1 = new Date(2016, 9, 30); // The issue date of the scheme
        private readonly Date _date2 = new Date(2017, 9, 30); // The first performance measurment date

        private readonly Date
            _date3 = new Date(2018, 9, 30); // The second performance measurement date and the payment date.

        private readonly double threshAbs = 0.10; // AAA share must return at least 10% each year 
        private readonly double threshRel = 0.03; // AA share must outperform the ALSI by at least 3% in each year

        /// <summary>
        /// When the product is created it must tell the model which share prices it needs and
        /// on what dates it needs them.
        /// </summary>
        public ProductWrapperEquitySample1() : base(TestHelpers.ZAR)
        {
            SetRequired(_aaa, _date1);
            SetRequired(_aaa, _date2);
            SetRequired(_aaa, _date3);
            SetRequired(_alsi, _date1);
            SetRequired(_alsi, _date2);
            SetRequired(_alsi, _date3);
            // cashflow dates
            var cfDates = new List<Date>();
            cfDates.Add(_date3);
            SetCashflowDates(cfDates);
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            double w1;
            double w2;
            var year1AaaReturn = Get(_aaa, _date2) / Get(_aaa, _date1) - 1;
            var year2AaaReturn = Get(_aaa, _date3) / Get(_aaa, _date2) - 1;
            var year1AlsiReturn = Get(_alsi, _date2) / Get(_alsi, _date1) - 1;
            var year2AlsiReturn = Get(_alsi, _date3) / Get(_alsi, _date2) - 1;
            if (year1AaaReturn > threshAbs && year2AaaReturn > threshAbs)
                w1 = 1.0;
            else
                w1 = 0.0;

            if (year1AaaReturn - year1AlsiReturn > threshRel && year2AaaReturn - year2AlsiReturn > threshRel)
                w2 = 1.0;
            else
                w2 = 0.0;

            return new List<Cashflow> {new Cashflow(_date3, Get(_aaa, _date3) * (w1 + w2), TestHelpers.ZAR)};
        }
    }

    
    public class ProductWrapperEquitySample2 : ProductWrapper
    {
        private readonly Share _aaa = new Share("AAA", TestHelpers.ZAR);
        private readonly Share _alsi = new Share("ALSI", TestHelpers.ZAR);
        private readonly Date _date1 = new Date(2016, 9, 30); // The issue date of the scheme
        private readonly Date _date2 = new Date(2017, 9, 30); // The first performance measurment date

        private readonly Date
            _date3 = new Date(2018, 9, 30); // The second performance measurement date and the payment date.

        private const double ThreshAbs = 0.10; // AAA share must return at least 10% each year 
        private const double ThreshRel = 0.03; // AA share must outperform the ALSI by at least 3% in each year

        public ProductWrapperEquitySample2()
        {
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            double w1;
            double w2;
            var year1AaaReturn = Get(_aaa, _date2) / Get(_aaa, _date1) - 1;
            var year2AaaReturn = Get(_aaa, _date3) / Get(_aaa, _date2) - 1;
            var year1AlsiReturn = Get(_alsi, _date2) / Get(_alsi, _date1) - 1;
            var year2AlsiReturn = Get(_alsi, _date3) / Get(_alsi, _date2) - 1;
            if (year1AaaReturn > ThreshAbs && year2AaaReturn > ThreshAbs)
                w1 = 1.0;
            else
                w1 = 0.0;

            if (year1AaaReturn - year1AlsiReturn > ThreshRel && year2AaaReturn - year2AlsiReturn > ThreshRel)
                w2 = 1.0;
            else
                w2 = 0.0;

            return new List<Cashflow> {new Cashflow(_date3, Get(_aaa, _date3) * (w1 + w2), TestHelpers.ZAR)};
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
            var shares = new [] {new Share("ALSI", TestHelpers.ZAR), new Share("AAA", TestHelpers.ZAR), new Share("BBB", TestHelpers.ZAR)};
            double[] prices = {200, 50, 100};
            double[] vols = {0.22, 0.52, 0.4};
            double[] divYields = {0.03, 0.0, 0.0};
            double[,] correlations =
            {
                {1.0, 0.5, 0.5},
                {0.5, 1.0, 0.5},
                {0.5, 0.5, 1.0}
            };
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, anchorDate,
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