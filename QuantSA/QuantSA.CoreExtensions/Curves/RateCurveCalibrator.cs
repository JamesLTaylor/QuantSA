using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.CoreExtensions.Curves
{
    public interface IRateCurveInstrument
    {
        void SetCalibrationDate(Date calibrationDate);

        Date GetCurveDate();

        double GetCurveEstimateDf();

        double Error(IMarketDataContainer marketData);
    }

    public class DepoCurveInstrument : IRateCurveInstrument
    {
        private readonly Currency _currency;
        private readonly double _simpleRate;
        private readonly Tenor _tenor;

        public DepoCurveInstrument(Currency currency, Tenor tenor, double simpleRate)
        {
            _currency = currency;
            _tenor = tenor;
            _simpleRate = simpleRate;
        }

        public void SetCalibrationDate(Date calibrationDate)
        {
            throw new NotImplementedException();
        }

        public Date GetCurveDate()
        {
            throw new NotImplementedException();
        }

        public double GetCurveEstimateDf()
        {
            throw new NotImplementedException();
        }

        public double Error(IMarketDataContainer marketData)
        {
            var curve = marketData.Get(new DiscountingSourceDescription(_currency));
            return 0.0;
        }
    }

    public class RateCurveCalibrator
    {
    }
}