using System;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;

namespace QuantSA.CoreExtensions.Curves.Instruments
{
    public class DepoCurveInstrument : IRateCurveInstrument
    {
        private readonly DiscountingSourceDescription _discountCurve;
        private readonly double _simpleRate;
        private readonly Tenor _tenor;

        private Date _calibrationDate;
        private double _cf;
        private IDiscountingSource _curve;
        private Date _maturityDate;

        public DepoCurveInstrument(Tenor tenor, double simpleRate, DiscountingSourceDescription discountCurve)
        {
            _tenor = tenor;
            _simpleRate = simpleRate;
            _discountCurve = discountCurve;
        }

        public string GetName()
        {
            return $"Deposit.{_tenor}.[{_discountCurve.Currency}]";
        }

        public void SetCalibrationDate(Date calibrationDate)
        {
            _calibrationDate = calibrationDate;
            _maturityDate = calibrationDate.AddTenor(_tenor);
            var yf = Actual365Fixed.Instance.YearFraction(_calibrationDate, _maturityDate);
            _cf = 1e6 * (1 + _simpleRate * yf);
        }

        public void SetMarketData(IMarketDataContainer marketData)
        {
            _curve = marketData.Get(_discountCurve);
        }

        public double Objective()
        {
            var df = _curve.GetDF(_maturityDate);
            return df * _cf - 1e6;
        }

        public Tuple<string, Date, double> GetInitialValue()
        {
            return new Tuple<string, Date, double>(_discountCurve.Name, _maturityDate, _simpleRate);
        }
    }
}