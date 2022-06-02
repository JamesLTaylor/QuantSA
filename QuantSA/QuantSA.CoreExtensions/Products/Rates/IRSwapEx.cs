using System;
using System.Collections.Generic;
using System.Text;
using QuantSA.Core.MarketData;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Rates;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Shared.Primitives;

namespace QuantSA.CoreExtensions.Products.Rates
{
    public class IRSwapEx
    {
        public static double ValueSwap(IRSwap swap, Date valueDate, IDiscountingSource curve)
        {
            // Get the required objects off the map                                                
            var index = swap.GetFloatingIndex();

            // Calculate the first fixing off the curve to use at all past dates.
            var df1 = curve.GetDF(valueDate);
            var laterDate = valueDate.AddTenor(index.Tenor);
            var df2 = curve.GetDF(laterDate);
            var dt = (laterDate - valueDate) / 365.0;
            var rate = (df1 / df2 - 1) / dt;

            //Set up the valuation engine.
            IFloatingRateSource forecastCurve = new ForecastCurveFromDiscount(curve, index,
                new FloatingRateFixingCurve1Rate(valueDate, rate, index));
            var curveSim = new DeterministicCurves(curve);
            curveSim.AddRateForecast(forecastCurve);
            var coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

            // Run the valuation
            var value = coordinator.Value(new Product[] { swap }, valueDate);
            return value;
        }

        public static double[] SwapZeroRisk(IRSwap swap, Date valueDate, Date[] dates, double[] rates, Currency currency)
        {
            // set the base curve
            for (var i = 1; i < dates.Length; i++)
                if (dates[i].value <= dates[i - 1].value)
                    throw new ArgumentException("Dates must be strictly increasing");
            var basecurve = new DatesAndRates(currency, dates[0], dates, rates);

            // Calculate the base value of the swap 
            var basevalue = ValueSwap(swap, valueDate, basecurve);

            double shift = 0.0001;
            var temp = new double[dates.Length];
            var PV01s = new double[dates.Length];
            for (int i = 0; i < dates.Length; i++)
            {
                rates.CopyTo(temp, 0);
                temp[i] += shift;

                PV01s[i] = ValueSwap(swap, valueDate, new DatesAndRates(currency, dates[0], dates, temp)) - basevalue;
            }

            return PV01s;
        }
    }
}
