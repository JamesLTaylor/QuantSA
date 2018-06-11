using System;
using System.Linq;
using Accord.Math;
using MathNet.Numerics.Interpolation;
using QuantSA.General.Dates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;

namespace QuantSA.General
{
    /// <summary>
    /// Curve with Linear Interpolation in continuously compounded rates.  Can be 
    /// composed as a spread over another discounting curve to ensure that the shape
    /// of the underlying curve is maintained.
    /// </summary>
    /// <seealso cref="IDiscountingSource" />
    public class ZeroRatesCurveForStripping : ICurveForStripping, IDiscountingSource
    {
        private readonly Date anchorDate;


        private readonly Currency ccy;
        private int dateOffset;
        private Date[] dates;
        private double[] dateValues;
        private LinearSpline spline;
        private readonly IDiscountingSource underlyingCurve;

        public ZeroRatesCurveForStripping(Date anchorDate, Currency ccy)
        {
            this.anchorDate = anchorDate;
            this.ccy = ccy;
            underlyingCurve = new TrivialCurve(anchorDate, ccy);
        }

        public ZeroRatesCurveForStripping(Date anchorDate, IDiscountingSource underlyingCurve)
        {
            this.anchorDate = anchorDate;
            this.underlyingCurve = underlyingCurve;
            ccy = underlyingCurve.GetCurrency();
        }

        public double[] rates { get; set; }

        public void SetDates(Date[] dates)
        {
            if (dates[0] > anchorDate)
            {
                var dateList = dates.ToList();
                dateList.Insert(0, anchorDate);
                this.dates = dateList.ToArray();
                dateOffset = 1;
            }
            else
            {
                this.dates = dates;
            }

            dateValues = this.dates.GetValues();
            rates = Vector.Ones(this.dates.Length).Multiply(0.02);
            spline = LinearSpline.InterpolateSorted(dateValues, rates);
        }

        public double[] GetRates()
        {
            if (dateOffset == 0) return rates;
            return rates.Get(1, rates.Length);
        }

        public void SetRate(int index, double rate)
        {
            if (dateOffset == 1 && index == 0) rates[0] = rate;
            rates[index + dateOffset] = rate;
            spline = LinearSpline.InterpolateSorted(dateValues, rates);
        }

        public double GetDF(Date date)
        {
            var rate = spline.Interpolate(date);
            var df = Math.Exp(-rate * (date - anchorDate.value) / 365.0);
            return underlyingCurve.GetDF(date) * df;
        }

        public Date GetAnchorDate()
        {
            return anchorDate;
        }

        public Currency GetCurrency()
        {
            return ccy;
        }

        private class TrivialCurve : IDiscountingSource
        {
            private readonly Date anchorDate;
            private readonly Currency ccy;

            public TrivialCurve(Date anchorDate, Currency currency)
            {
                ccy = currency;
                this.anchorDate = anchorDate;
            }

            public Date GetAnchorDate()
            {
                return anchorDate;
            }

            public Currency GetCurrency()
            {
                return ccy;
            }

            public double GetDF(Date date)
            {
                return 1.0;
            }
        }
    }
}