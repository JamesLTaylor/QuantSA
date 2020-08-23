using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products.Rates;
using QuantSA.CoreExtensions.Data;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;
using QuantSA.Shared.State;

namespace QuantSA.Excel.Addin.Tests
{
    /// <summary>
    /// A partial duplicate of the test helpers in the main library. Repeated here because this project targets
    /// .NET framework while the other targets .NET Core
    /// </summary>
    public static class ExcelTestHelpers
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

        public static readonly Currency ZAR = SetAndReturn(new Currency("ZAR"));
        public static FloatRateIndex Jibar3M => SetAndReturn(new FloatRateIndex("ZAR.JIBAR.3M", ZAR, "JIBAR", Tenor.FromMonths(3)));


        public static IRSwap ZARSwap()
        {
            var quarters = 8;
            var indexDates = new Date[quarters];
            var paymentDates = new Date[quarters];
            var spreads = new double[quarters];
            var accrualFractions = new double[quarters];
            var notionals = new double[quarters];
            var fixedRate = 0.07;
            var ccy = ZAR;

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