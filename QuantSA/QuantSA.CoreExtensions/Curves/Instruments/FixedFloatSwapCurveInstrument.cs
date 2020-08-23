using System;
using System.Linq;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products.Rates;
using QuantSA.CoreExtensions.ProductPVs.Rates;
using QuantSA.CoreExtensions.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Curves.Instruments
{
    public class FixedFloatSwapCurveInstrument : IRateCurveInstrument
    {
        public enum CurveToStrip
        {
            Forecast,
            Discount
        }

        private readonly DiscountingSourceDescription _discountCurveDescription;
        private readonly double _fixedRate;
        private readonly FloatRateIndex _index;
        private readonly string _nameOfCurveToStrip;
        private readonly double _spread;
        private readonly Tenor _tenor;
        private IDiscountingSource _discountCurve;
        private Date _endDate;
        private FixedLeg _fixedLeg;

        private FloatLeg _floatLeg;
        private IFloatingRateSource _forecastCurve;


        public FixedFloatSwapCurveInstrument(Tenor tenor, FloatRateIndex index, double spread, double fixedRate,
            DiscountingSourceDescription discountCurveDescription, CurveToStrip curveToStrip)
        {
            _tenor = tenor;
            _index = index;
            _spread = spread;
            _fixedRate = fixedRate;
            _discountCurveDescription = discountCurveDescription;
            switch (curveToStrip)
            {
                case CurveToStrip.Forecast:
                {
                    _nameOfCurveToStrip = new FloatingRateSourceDescription(index).Name;
                    break;
                }
                case CurveToStrip.Discount:
                {
                    _nameOfCurveToStrip = _discountCurveDescription.Name;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(curveToStrip), curveToStrip, null);
            }
        }

        public string GetName()
        {
            return $"FixedFloatSwap.{_tenor}.[{_index}]";
        }

        public void SetCalibrationDate(Date calibrationDate)
        {
            _floatLeg = SwapFactory.CreateFloatLeg(calibrationDate, _tenor, _index, _spread);
            _fixedLeg = SwapFactory.CreateFixedLeg(calibrationDate, _tenor, _index, _fixedRate);
            _floatLeg.SetValueDate(calibrationDate);
            _fixedLeg.SetValueDate(calibrationDate);
            _endDate = _floatLeg.GetCashflowDates(_index.Currency).Max();
            var otherEndDate = _fixedLeg.GetCashflowDates(_index.Currency).Max();
            if (_endDate < otherEndDate) _endDate = otherEndDate;
        }

        public void SetMarketData(IMarketDataContainer marketData)
        {
            _forecastCurve = marketData.Get(new FloatingRateSourceDescription(_index));
            _discountCurve = marketData.Get(_discountCurveDescription);
        }

        public double Objective()
        {
            var value1 = _fixedLeg.GetCFs().PV(_discountCurve);
            var value2 = _floatLeg.CurvePV(_forecastCurve, _discountCurve);
            return value1 - value2;
        }

        public Tuple<string, Date, double> GetInitialValue()
        {
            return new Tuple<string, Date, double>(_nameOfCurveToStrip, _endDate, 0.07);
        }
    }
}