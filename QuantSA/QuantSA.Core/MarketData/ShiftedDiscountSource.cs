using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.General
{
    public class ShiftedDiscountSource : IDiscountingSource
    {
        private Date[] dates;
        private readonly double effectiveRateBump;
        private double[] effectiveRateBumps;
        private readonly bool hasParallelShift;
        private readonly IDiscountingSource underlyingCurve;

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
            var df = underlyingCurve.GetDF(date);
            double adjustedDF;
            if (hasParallelShift)
                adjustedDF = df * Math.Exp(effectiveRateBump * (date - underlyingCurve.GetAnchorDate()) / 365.0);
            else
                adjustedDF = df * 1.0;
            return adjustedDF;
        }
    }
}