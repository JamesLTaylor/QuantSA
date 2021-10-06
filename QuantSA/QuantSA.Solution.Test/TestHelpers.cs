using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products.Rates;
using QuantSA.CoreExtensions.Data;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;
using QuantSA.Shared.State;
using QuantSA.Shared.MarketData;
using System.Collections.Generic;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Conventions.BusinessDay;
using System.Linq;
using QuantSA.Core.Products.SAMarket;
using QuantSA.Core.MarketData;


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
        public static FloatRateIndex Jibar1D => SetAndReturn(new FloatRateIndex("ZAR.JIBAR.1D", ZAR, "JIBAR", Tenor.FromDays(1)));
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

        public static AssetSwap CreateAssetSwap(double payFixed, BesaJseBond besaJseBond, Date settleDate, FloatRateIndex index, double spread, Calendar calendar, Currency ccy, IDiscountingSource discountCurve)
        {
            //Design floating leg inputs
            var dayCount = Actual365Fixed.Instance;
            var unAdjResetDatesFloating = new List<Date>();
            var unAdjPaymentDatesFloating = new List<Date>();
            var resetDatesFloating = new List<Date>();
            var paymentDatesFloating = new List<Date>();
            var accrualFractions = new List<double>();
            var endDate = besaJseBond.maturityDate;
            var paymentDateFloating = new Date(endDate);
            var resetDateFloating = paymentDateFloating.SubtractTenor(index.Tenor);
            while (resetDateFloating >= settleDate)
            {
                unAdjPaymentDatesFloating.Add(paymentDateFloating);
                unAdjResetDatesFloating.Add(resetDateFloating);
                resetDatesFloating.Add(BusinessDayStore.ModifiedFollowing.Adjust(resetDateFloating, calendar));
                paymentDatesFloating.Add(BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFloating, calendar));
                accrualFractions.Add(dayCount.YearFraction(BusinessDayStore.ModifiedFollowing.Adjust(resetDateFloating, calendar), BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFloating, calendar)));
                paymentDateFloating = new Date(resetDateFloating);
                resetDateFloating = paymentDateFloating.SubtractTenor(index.Tenor);
            }

            resetDatesFloating.Reverse();
            paymentDatesFloating.Reverse();
            accrualFractions.Reverse();

            resetDatesFloating[0] = new Date(settleDate);
            var firstResetDate = resetDatesFloating.First();
            var firstPaymentDate = paymentDatesFloating.First();
            accrualFractions[0] = dayCount.YearFraction(firstResetDate, firstPaymentDate);

            //Design Fixed leg inputs
            var unAdjPaymentDatesFixed = new List<Date>();
            var paymentDatesFixed = new List<Date>();

            var thisYearCpn1 = new Date(settleDate.Year, besaJseBond.couponMonth1, besaJseBond.couponDay1);
            var thisYearCpn2 = new Date(settleDate.Year, besaJseBond.couponMonth2, besaJseBond.couponDay2);
            var lastYearCpn2 = new Date(settleDate.Year - 1, besaJseBond.couponMonth2, besaJseBond.couponDay2);

            Date lcd; //lcd stands for last coupon date
            if (settleDate > thisYearCpn2)
                lcd = new Date(thisYearCpn2.Year, thisYearCpn2.Month, thisYearCpn2.Day);
            if (settleDate > thisYearCpn1)
                lcd = new Date(thisYearCpn1.Year, thisYearCpn1.Month, thisYearCpn1.Day);
            lcd = new Date(lastYearCpn2.Year, lastYearCpn2.Month, lastYearCpn2.Day);

            Date ncd; //ncd stands for next coupon date
            if (lcd.Month == besaJseBond.couponMonth2)
                ncd = new Date(lcd.Year + 1, besaJseBond.couponMonth1, besaJseBond.couponDay1);
            else
                ncd = new Date(lcd.Year, besaJseBond.couponMonth2, besaJseBond.couponDay2);

            var paymentDateFixed = new Date(ncd.AddTenor(Tenor.FromMonths(6)));

            while (paymentDateFixed <= endDate)
            {
                unAdjPaymentDatesFixed.Add(paymentDateFixed);
                paymentDatesFixed.Add(BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFixed, calendar));
                paymentDateFixed = paymentDateFixed.AddTenor(Tenor.FromMonths(6));
            }

            //create new instance of asset swap
            var assetSwap = new AssetSwap(payFixed, index, besaJseBond, resetDatesFloating, paymentDatesFloating, paymentDatesFixed, spread,
                accrualFractions, calendar, ccy);

            //Create trade date of the swap
            var unAdjTradeDate = settleDate.AddDays(-3);
            var tradeDate = BusinessDayStore.ModifiedFollowing.Adjust(unAdjTradeDate, assetSwap.zaCalendar);

            //Set value date
            assetSwap.SetValueDate(tradeDate);

            // Calculate the first fixing off the curve to use at all past dates.
            var df1 = discountCurve.GetDF(tradeDate);
            var laterDate = tradeDate.AddTenor(assetSwap.index.Tenor);
            var df2 = discountCurve.GetDF(laterDate);
            var dt = (laterDate - tradeDate) / 365.0;
            var rate = (df1 / df2 - 1) / dt;

            // Create the forecast curve from the discount curve
            IFloatingRateSource forecastCurve = new ForecastCurveFromDiscount(discountCurve, assetSwap.index,
                new FloatingRateFixingCurve1Rate(tradeDate, rate, assetSwap.index));

            //Setting index values
            var indexValues = new double[assetSwap.indexDates.Count];
            for (var i = 0; i < assetSwap.indexDates.Count; i++)
                indexValues[i] = forecastCurve.GetForwardRate(assetSwap.indexDates[i]);
            assetSwap.SetIndexValues(assetSwap.index, indexValues);

            return assetSwap;
        }
    }
}