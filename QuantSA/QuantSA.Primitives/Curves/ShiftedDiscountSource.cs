using System;
using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.Curves
{
    public class ShiftedDiscountSource : IDiscountingSource
    {
        private bool hasParallelShift; 
        private double effectiveRateBump;
        private IDiscountingSource underlyingCurve;
        private double[] effectiveRateBumps;
        private Date[] dates;

        public ShiftedDiscountSource(IDiscountingSource underlyingCurve, double effectiveRateBump)
        {
            this.underlyingCurve = underlyingCurve;
            this.effectiveRateBump = effectiveRateBump;
            hasParallelShift = true;
        }

        public ShiftedDiscountSource(IDiscountingSource underlyingCurve, Date[] dates, double[] effectiveRateBumps)
        {
            this.underlyingCurve = underlyingCurve;            
            this.dates = dates;
            this.effectiveRateBumps = effectiveRateBumps;
            hasParallelShift = false;
        }

        public Date GetAnchorDate()
        {
            return underlyingCurve.GetAnchorDate();
        }

        public Currency GetCurrency()
        {
            return underlyingCurve.GetCurrency();
        }

        public double GetDF(Date date)
        {
            double df = underlyingCurve.GetDF(date);
            double adjustedDF;
            if (hasParallelShift)
            {
                adjustedDF = df * Math.Exp(effectiveRateBump * (date - underlyingCurve.GetAnchorDate()) / 365.0);
            }
            else
            {
                //TODO: get the interpolated shift size from the dates and bumps
                adjustedDF = df * 1.0;
            }
            return adjustedDF;
        }
    }
}
