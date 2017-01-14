using Accord.Math;
using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A curve that can be used in bootstrapping a forward rate source.  The forward rates
    /// themselves are linearly interpolated and are not based on discount factors.  The curve 
    /// can be made to follow the shape of an underlying curve, <see cref="underlyingCurve"/>.
    /// </summary>
    /// <seealso cref="QuantSA.General.ICurveForStripping" />
    /// <seealso cref="QuantSA.General.IFloatingRateSource" />
    public class ForwardRatesCurveForStripping : ICurveForStripping, IFloatingRateSource
    {
        /// <summary>
        /// An <see cref="IFloatingRateSource"/> that always returns zero.
        /// </summary>
        /// <seealso cref="QuantSA.General.IFloatingRateSource" />
        private class ZeroFloatingRates : IFloatingRateSource
        {
            private FloatingIndex index;
            public ZeroFloatingRates(FloatingIndex index) { this.index = index; }
            public FloatingIndex GetFloatingIndex() { return index; }
            public double GetForwardRate(Date date) { return 0.0; }
        }

        /// <summary>
        /// An <see cref="IFloatingRateSource"/> that always returns the forward rates implied by a
        /// discount curve.  Takes a shortcut in calculating the forward rate from discount factors by adding
        /// 3 months with no date adjustments and assuming actual 365 and simple compounding for the implied rate.
        /// </summary>
        /// <seealso cref="QuantSA.General.IFloatingRateSource" />
        private class DiscountBasedFloatingRates : IFloatingRateSource
        {
            private FloatingIndex index;
            private IDiscountingSource discountingSource;
            public DiscountBasedFloatingRates(FloatingIndex index, IDiscountingSource discountingSource)
            {
                this.index = index;
                this.discountingSource = discountingSource;
            }

            public FloatingIndex GetFloatingIndex()
            {
                return index;
            }

            public double GetForwardRate(Date date)
            {
                Date endDate = date.AddTenor(index.tenor);
                double df1 = discountingSource.GetDF(date);
                double df2 = discountingSource.GetDF(endDate);
                double yearFrac = (endDate - date) / 365.0;
                return (df1 / df2 - 1) / yearFrac;
            }
        }

        FloatingIndex index;
        Date anchorDate;
        private Date[] dates;
        private double[] dateValues;
        private LinearSpline spline;
        public double[] rates { get; set; }
        IFloatingRateSource underlyingCurve;
        int dateOffset = 0;

        public ForwardRatesCurveForStripping(Date anchorDate, FloatingIndex index)
        {
            this.anchorDate = anchorDate;
            this.index = index;
            underlyingCurve = new ZeroFloatingRates(index);
        }

        public ForwardRatesCurveForStripping(Date anchorDate, FloatingIndex index, IDiscountingSource underlyingCurve)
        {
            this.anchorDate = anchorDate;
            this.index = index;
            this.underlyingCurve = new DiscountBasedFloatingRates(index, underlyingCurve);
        }

        public ForwardRatesCurveForStripping(Date anchorDate, FloatingIndex index, IFloatingRateSource underlyingCurve)
        {
            this.anchorDate = anchorDate;
            this.index = index;
            this.underlyingCurve = underlyingCurve;
        }

        public FloatingIndex GetFloatingIndex()
        {
            return index;
        }

        public double GetForwardRate(Date date)
        {
            double rate = spline.Interpolate(date);            
            return underlyingCurve.GetForwardRate(date) + rate;
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
            if (dateOffset == 0)
            {
                return rates;
            }
            return rates.Get(1, rates.Length);
        }

        public void SetRate(int index, double rate)
        {
            if (dateOffset == 1 && index == 0)
            {
                rates[0] = rate;
            }
            rates[index + dateOffset] = rate;
            spline = LinearSpline.InterpolateSorted(dateValues, rates);
        }
    }
}
