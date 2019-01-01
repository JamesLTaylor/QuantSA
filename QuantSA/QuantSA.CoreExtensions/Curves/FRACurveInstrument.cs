using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Curves
{
    public class FRACurveInstrument : IRateCurveInstrument
    {
        private readonly Tenor _endTenor;
        private readonly FloatingRateSourceDescription _floatingRateSourceDescription;
        private readonly double _simpleRate;
        private readonly Tenor _startTenor;

        private IFloatingRateSource _curve;
        private Date _endDate;
        private Date _startDate;

        public FRACurveInstrument(Tenor startTenor, Tenor endTenor, FloatRateIndex floatRateIndex, double simpleRate)
        {
            _startTenor = startTenor;
            _endTenor = endTenor;
            _simpleRate = simpleRate;
            _floatingRateSourceDescription = new FloatingRateSourceDescription(floatRateIndex);
        }

        public void SetCalibrationDate(Date calibrationDate)
        {
            _startDate = calibrationDate.AddTenor(_startTenor);
            _endDate = calibrationDate.AddTenor(_endTenor);
        }

        public void SetMarketData(IMarketDataContainer marketData)
        {
            _curve = marketData.Get(_floatingRateSourceDescription);
        }

        public double Objective()
        {
            return 1e6 * (_curve.GetForwardRate(_startDate) - _simpleRate);
        }

        public Tuple<string, Date, double> GetInitialValue()
        {
            return new Tuple<string, Date, double>(_floatingRateSourceDescription.Name, _endDate, _simpleRate);
        }
    }
}