using System;
using System.Linq;
using MathNet.Numerics.Optimization;
using QuantSA.Core.Products.Rates;
using QuantSA.CoreExtensions.ProductPVs.Rates;
using QuantSA.CoreExtensions.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Curves
{
    /// <summary>
    /// A swap with two floating legs to be used in curve stripping.
    /// </summary>
    public class BasisSwapCurveInstrument : IRateCurveInstrument
    {
        public enum CurveToStrip
        {
            Leg1Forecast,
            Leg2Forecast,
            DiscountCurve
        }

        private readonly string _curveName;
        private readonly DiscountingSourceDescription _discountCurveDescription;
        private readonly FloatRateIndex _leg1Index;
        private readonly double _leg1Spread;
        private readonly FloatRateIndex _leg2Index;
        private readonly double _leg2Spread;
        private readonly Tenor _tenor;
        private IDiscountingSource _discountCurve;
        private Date _endDate;

        private FloatLeg _leg1;
        private IFloatingRateSource _leg1Curve;
        private FloatLeg _leg2;
        private IFloatingRateSource _leg2Curve;

        public BasisSwapCurveInstrument(Tenor tenor, FloatRateIndex leg1Index, FloatRateIndex leg2Index,
            double leg1Spread,
            double leg2Spread, DiscountingSourceDescription discountCurveDescription, CurveToStrip curveToStrip)
        {
            _tenor = tenor;
            _leg1Index = leg1Index;
            _leg2Index = leg2Index;
            _leg1Spread = leg1Spread;
            _leg2Spread = leg2Spread;
            _discountCurveDescription = discountCurveDescription;
            switch (curveToStrip)
            {
                case CurveToStrip.Leg1Forecast:
                {
                    _curveName = new FloatingRateSourceDescription(leg1Index).Name;
                    break;
                }
                case CurveToStrip.Leg2Forecast:
                {
                    _curveName = new FloatingRateSourceDescription(leg2Index).Name;
                    break;
                }
                case CurveToStrip.DiscountCurve:
                {
                    _curveName = _discountCurveDescription.Name;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(curveToStrip), curveToStrip, null);
            }
        }

        public void SetCalibrationDate(Date calibrationDate)
        {
            _leg1 = SwapFactory.CreateFloatLeg(calibrationDate, _tenor, _leg1Index, _leg1Spread);
            _leg2 = SwapFactory.CreateFloatLeg(calibrationDate, _tenor, _leg2Index, _leg2Spread);
            _leg1.SetValueDate(calibrationDate);
            _leg2.SetValueDate(calibrationDate);
            _endDate = _leg1.GetCashflowDates(_leg1Index.Currency).Max();
            var otherEndDate = _leg2.GetCashflowDates(_leg2Index.Currency).Max();
            if (_endDate < otherEndDate) _endDate = otherEndDate;
        }

        public void SetMarketData(IMarketDataContainer marketData)
        {
            _leg1Curve = marketData.Get(new FloatingRateSourceDescription(_leg1Index));
            _leg2Curve = marketData.Get(new FloatingRateSourceDescription(_leg2Index));
            _discountCurve = marketData.Get(_discountCurveDescription);
        }

        public double Objective()
        {
            var value1 = _leg1.CurvePV(_leg1Curve, _discountCurve);
            var value2 = _leg1.CurvePV(_leg2Curve, _discountCurve);
            return value1 - value2;
        }

        public Tuple<string, Date, double> GetInitialValue()
        {
            return new Tuple<string, Date, double>(_curveName, _endDate, 0.07);
        }
    }
}