using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;

namespace QuantSA.Solution.Test
{
    public static class TestHelpers
    {
        private static TestSharedData _sharedData;

        public static TestSharedData SharedData
        {
            get
            {
                if (_sharedData != null) return _sharedData;
                _sharedData = new TestSharedData();
                QuantSAState.SetSharedData(_sharedData);

                return _sharedData;
            }
        }
        
        public static readonly Date AnchorDate = new Date("2018-06-23");
        public static readonly Currency ZAR = new Currency("ZAR");

        public static ReferenceEntity TestCp
        {
            get
            {
                if (SharedData.TryGet<ReferenceEntity>("ABC", out var refEntity)) return refEntity;
                refEntity = new ReferenceEntity("ABC");
                SharedData.Set(refEntity);
                return refEntity;
            }
        }

        public static IRSwap ZARSwap()
        {
            return IRSwap.CreateZARSwap(0.07, true, 100, AnchorDate, Tenor.FromYears(2));
        }

        public static DatesAndRates FlatDiscountCurve()
        {
            var dates = new[] {AnchorDate, AnchorDate.AddMonths(120)};
            var rates = new[] {0.07, 0.07};
            return new DatesAndRates(ZAR, AnchorDate, dates, rates);
        }
    }
}