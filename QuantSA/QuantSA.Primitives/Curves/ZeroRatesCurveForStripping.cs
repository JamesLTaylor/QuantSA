using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using MathNet.Numerics.Interpolation;
using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.Curves
{

    /// <summary>
    /// Curve with Linear Interpolation in continuously compounded rates.  Can be 
    /// composed as a spread over another discounting curve to ensure that the shape
    /// of the underlying curve is maintained.
    /// </summary>
    /// <seealso cref="IDiscountingSource" />
    public class ZeroRatesCurveForStripping : ICurveForStripping, IDiscountingSource
    {
        private class TrivialCurve : IDiscountingSource
        {
            Currency ccy;
            Date anchorDate;
            public TrivialCurve(Date anchorDate, Currency currency)
            {
                this.ccy = currency;
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


        Currency ccy;
        Date anchorDate;
        private Date[] dates;
        private double[] dateValues;
        private LinearSpline spline;
        public double[] rates { get; set; }
        IDiscountingSource underlyingCurve;
        int dateOffset = 0;

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

        public void SetDates(Date[] dates)
        {
            if (dates[0] > anchorDate)
            {
                List<Date> dateList = dates.ToList();
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
            if (dateOffset==0)
            {
                return rates;
            }
            return rates.Get(1, rates.Length);
        }

        public void SetRate(int index, double rate)
        {
            if (dateOffset==1 && index==0)
            {
                rates[0] = rate;
            }
            rates[index+dateOffset] = rate;
            spline = LinearSpline.InterpolateSorted(dateValues, rates);
        }
        
        public double GetDF(Date date)
        {
            double rate = spline.Interpolate(date);            
            double df = Math.Exp(-rate * (date - anchorDate.value) / 365.0);
            return underlyingCurve.GetDF(date)*df;
        }

        public Date GetAnchorDate()
        {
            return anchorDate;
        }

        public Currency GetCurrency()
        {
            return ccy;
        }
    }
}
