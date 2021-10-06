using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.MarketObservables;
using System.Collections.Generic;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Conventions.BusinessDay;
using System.Linq;
using QuantSA.Shared.Primitives;
using QuantSA.Core.MarketData;
using QuantSA.Shared.MarketData;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products.SAMarket;
using QuantSA.CoreExtensions.SAMarket;


namespace QuantSA.CoreExtensions.Products.Rates
{
    public static class AssetSwapEx
    {
        public static ResultStore AssetSwapMeasures(this AssetSwap assetSwap, Date settleDate, double ytm, IDiscountingSource discountCurve)
        {
            //Create Asset Swap
            var swap = CreateAssetSwap(assetSwap.payFixed, assetSwap.underlyingBond, settleDate, assetSwap.index, assetSwap.spread, assetSwap.zaCalendar,
                assetSwap.ccy, discountCurve);

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
            var indexValues = new double[swap.indexDates.Count];
            for (var i = 0; i < swap.indexDates.Count; i++)
                indexValues[i] = forecastCurve.GetForwardRate(swap.indexDates[i]);
            assetSwap.SetIndexValues(assetSwap.index, indexValues);

            //Calculate present value of fixed and floating cashflows
            var numeratorCFs = swap.GetCFs().PV(discountCurve);

            //Calculate present value of denominator cashflows for spread equation
            var denomCFs = new List<Cashflow>();
            for (var i = 0; i < swap.paymentDatesFloating.Count; i++)
                if (i <= swap.paymentDatesFloating.Count)
                {
                    denomCFs.Add(new Cashflow(swap.paymentDatesFloating[i], -100 * swap.accrualFractions[i], swap.ccy));
                }

            var bondresults = assetSwap.underlyingBond.GetSpotMeasures(settleDate, ytm);
            var roundedAip = (double)bondresults.GetScalar(BesaJseBondEx.Keys.RoundedAip);
            var denominatorCFs = denomCFs.PV(discountCurve);

            var firstCF = new List<Cashflow>();
            for (var i = 0; i < 1; i++)
                if (i <= 1)
                {
                    firstCF.Add(new Cashflow(settleDate, (roundedAip - 100), swap.ccy));
                }
            var pvFirstCF = firstCF.PV(discountCurve);

            //This is the assetSwapSpread calculation
            var assetSwapSpread = (pvFirstCF + numeratorCFs) / denominatorCFs;

            var results = new ResultStore();
            results.Add(Keys.RoundedAip, roundedAip);
            results.Add(Keys.PVFirstCF, pvFirstCF);
            results.Add(Keys.NumeratorCashFlowsPrice, numeratorCFs);
            results.Add(Keys.DenominatorCashFlowsPrice, denominatorCFs);
            results.Add(Keys.AssetSwapSpread, assetSwapSpread);

            return results;
        }

        public static class Keys
        {
            public const string RoundedAip = "roundedAip";
            public const string PVFirstCF = "pvFirstCF";
            public const string NumeratorCashFlowsPrice = "numeratorCFs";
            public const string DenominatorCashFlowsPrice = "denominatorCFs";
            public const string AssetSwapSpread = "assetSwapSpread";
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
                accrualFractions.Add(dayCount.YearFraction(BusinessDayStore.ModifiedFollowing.Adjust(resetDateFloating, calendar),
                    BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFloating, calendar)));
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
