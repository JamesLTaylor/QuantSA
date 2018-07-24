using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;

namespace QuantSA.Solution.Test
{
    public static class TestHelperProducts
    {
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

            var date1 = new Date(TestHelpers.AnchorDate);

            for (var i = 0; i < quarters; i++)
            {
                var date2 = TestHelpers.AnchorDate.AddMonths(3 * (i + 1));
                indexDates[i] = new Date(date1);
                paymentDates[i] = new Date(date2);
                spreads[i] = 0.0;
                accrualFractions[i] = (date2 - date1) / 365.0;
                notionals[i] = 100;
                date1 = new Date(date2);
            }

            return new IRSwap(-1, indexDates, paymentDates, TestHelpers.Jibar3M, spreads, accrualFractions,
                notionals, fixedRate, ccy);
        }
    }
}