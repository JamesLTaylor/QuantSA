using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Conventions.BusinessDay;
using System.Linq;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.MarketData;
using System;
using QuantSA.Core.Formulae;

namespace QuantSA.CoreExtensions.Products.Rates
{
    public static class InflationLinkedSwapEx
    {
        public static ResultStore InflationLinkedSwapMeasures(this InflationLinkedSwap inflationLinkedSwap, Date[]cpiDates, double[] cpiRates, IFloatingRateSource forecastCurve)
        {
            //Create Inflation Swap
            var swap = CreateInflationLinkedSwap(inflationLinkedSwap.payFixed, inflationLinkedSwap.startDate, inflationLinkedSwap.nominal, inflationLinkedSwap.tenor,
                inflationLinkedSwap.fixedRate, inflationLinkedSwap.index, inflationLinkedSwap.spread, inflationLinkedSwap.zaCalendar, inflationLinkedSwap.ccy);

            //Set value date
            swap.SetValueDate(inflationLinkedSwap.startDate);

            //Set index values
            var indexValues = new double[swap.indexDates.Length];
            for (var i = 0; i < swap.indexDates.Length; i++)
                indexValues[i] = forecastCurve.GetForwardRate(swap.indexDates[i]);
            swap.SetIndexValues(swap.index, indexValues);

            //Determine swap end date
            var unAdjEndDate = inflationLinkedSwap.startDate.AddMonths(12 * inflationLinkedSwap.tenor.Years);
            var endDate = BusinessDayStore.ModifiedFollowing.Adjust(unAdjEndDate, inflationLinkedSwap.zaCalendar);

            //Calculate value of fixed and floating cashflows
            var floatingCashFlows = swap.GetCFs();
            var floatingLegCashFlows = floatingCashFlows.Last().Amount;

            var cpiStartDate = LaggedCPI.GetCPI(inflationLinkedSwap.startDate, cpiDates, cpiRates);
            var cpiEndDate = LaggedCPI.GetCPI(endDate, cpiDates, cpiRates);
                
            var fixedCashFlows = inflationLinkedSwap.payFixed * inflationLinkedSwap.nominal * Math.Pow((1 + inflationLinkedSwap.fixedRate / 2), 
                2 * (endDate - inflationLinkedSwap.startDate) / 365) * cpiEndDate / cpiStartDate;
                
            var netCashFlows = floatingLegCashFlows + fixedCashFlows;

            // Store results
            var results = new ResultStore();
            results.Add(Keys.FloatingLegCashFlows, floatingLegCashFlows);
            results.Add(Keys.FixedLegCashFlows, fixedCashFlows);
            results.Add(Keys.NetCashFlows, netCashFlows);

            return results;
        }

        public static class Keys
        {
            public const string FloatingLegCashFlows = "floatingLegCashFlows";
            public const string FixedLegCashFlows = "fixedLegCashFlows";
            public const string NetCashFlows = "netCashFlows";
        }

        public static InflationLinkedSwap CreateInflationLinkedSwap(double payFixed, Date startDate, double nominal, Tenor tenor, double fixedRate, 
            FloatRateIndex index, double spread, Calendar calendar, Currency ccy)
        {

            //Design floating leg inputs
            var quarters = tenor.Years * 4 + tenor.Months / 3;
            var resetDatesFloating = new Date[quarters];
            var paymentDatesFloating = new Date[quarters];
            var spreads = new double[quarters];
            var accrualFractions = new double[quarters];
            var notionals = new double[quarters];

            var date1 = new Date(startDate);

            for (var i = 0; i < quarters; i++)
            {
                var date2 = startDate.AddMonths(3 * (i + 1));
                var x = BusinessDayStore.ModifiedFollowing.Adjust(date1, calendar);
                resetDatesFloating[i] = new Date(x);
                var y = BusinessDayStore.ModifiedFollowing.Adjust(date2, calendar);
                paymentDatesFloating[i] = new Date(y);
                spreads[i] = 0.0;
                accrualFractions[i] = (date2 - date1) / 365.0;
                notionals[i] = nominal;
                date1 = new Date(date2);
            }

            //Create new instance of inflation linked swap
            var swap = new InflationLinkedSwap(payFixed, startDate, nominal, tenor, fixedRate, index, resetDatesFloating, paymentDatesFloating, spread,
                accrualFractions, calendar, ccy);

            return swap;
        }
    }
}

