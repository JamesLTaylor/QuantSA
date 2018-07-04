using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products.Rates;
using QuantSA.ProductExtensions.Data;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;

namespace QuantSA.Solution.Test
{
    public static class TestHelpers
    {
        private static SharedData _sharedData;

        public static SharedData SharedData
        {
            get
            {
                if (_sharedData != null) return _sharedData;
                _sharedData = new SharedData();
                QuantSAState.SetSharedData(_sharedData);

                return _sharedData;
            }
        }
        
        public static readonly Date AnchorDate = new Date("2018-06-23");

        private static T SetAndReturn<T>(T instance) where T : ISerializableViaName
        {
            if (SharedData.TryGet<T>(instance.GetName(), out var prevInstance)) return prevInstance;
            SharedData.Set(instance);
            return instance;
        }

        public static Currency ZAR = SetAndReturn(new Currency("ZAR"));
        public static Currency EUR = SetAndReturn(new Currency("EUR"));
        public static Currency USD = SetAndReturn(new Currency("USD"));
        public static CurrencyPair USDZAR = SetAndReturn(new CurrencyPair("USDZAR", USD, ZAR));
        public static CurrencyPair EURZAR = SetAndReturn(new CurrencyPair("EURZAR", EUR, ZAR));
        public static ReferenceEntity TestCp => SetAndReturn(new ReferenceEntity("ABC"));
        public static FloatRateIndex Jibar3M => SetAndReturn(new FloatRateIndex("ZAR.JIBAR.3M", ZAR, "JIBAR", Tenor.FromMonths(3)));
        public static FloatRateIndex Libor3M => SetAndReturn(new FloatRateIndex("USD.LIBOR.3M", USD, "LIBOR", Tenor.FromMonths(3)));
        public static FloatRateIndex Euribor3M => SetAndReturn(new FloatRateIndex("EUR.EURIBOR.3M", EUR, "EURIBOR", Tenor.FromMonths(3)));

        /// <summary>
        /// Constructor for ZAR market standard, fixed for float 3m Jibar swap.
        /// </summary>
        /// <param name="rate">The fixed rate paid or received</param>
        /// <param name="payFixed">Is the fixed rate paid?</param>
        /// <param name="notional">Flat notional for all dates.</param>
        /// <param name="startDate">First reset date of swap</param>
        /// <param name="tenor">Tenor of swap, must be a whole number of years.</param>
        /// <returns></returns>
        public static IRSwap CreateZARSwap(double rate, bool payFixed, double notional, Date startDate, Tenor tenor, FloatRateIndex jibar)
        {
            var quarters = tenor.Years * 4 + tenor.Months / 3;
            var indexDates = new Date[quarters];
            var paymentDates = new Date[quarters];
            var spreads = new double[quarters];
            var accrualFractions = new double[quarters];
            var notionals = new double[quarters];
            var fixedRate = rate;
            var ccy = TestHelpers.ZAR;

            var date1 = new Date(startDate);

            for (var i = 0; i < quarters; i++)
            {
                var date2 = startDate.AddMonths(3 * (i + 1));
                indexDates[i] = new Date(date1);
                paymentDates[i] = new Date(date2);
                spreads[i] = 0.0;
                accrualFractions[i] = (date2 - date1) / 365.0;
                notionals[i] = notional;
                date1 = new Date(date2);
            }

            var newSwap = new IRSwap(payFixed ? -1 : 1, indexDates, paymentDates, jibar, spreads, accrualFractions,
                notionals, fixedRate, ccy);
            return newSwap;
        }

        public static IRSwap ZARSwap()
        {
            var quarters = 8;
            var indexDates = new Date[quarters];
            var paymentDates = new Date[quarters];
            var spreads = new double[quarters];
            var accrualFractions = new double[quarters];
            var notionals = new double[quarters];
            var fixedRate = 0.07;
            var ccy = TestHelpers.ZAR;

            var date1 = new Date(AnchorDate);

            for (var i = 0; i < quarters; i++)
            {
                var date2 = AnchorDate.AddMonths(3 * (i + 1));
                indexDates[i] = new Date(date1);
                paymentDates[i] = new Date(date2);
                spreads[i] = 0.0;
                accrualFractions[i] = (date2 - date1) / 365.0;
                notionals[i] = 100;
                date1 = new Date(date2);
            }

            return new IRSwap(-1, indexDates, paymentDates, Jibar3M, spreads, accrualFractions,
                notionals, fixedRate, ccy);
        }

        public static DatesAndRates FlatDiscountCurve()
        {
            var dates = new[] {AnchorDate, AnchorDate.AddMonths(120)};
            var rates = new[] {0.07, 0.07};
            return new DatesAndRates(ZAR, AnchorDate, dates, rates);
        }
    }
}