using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class Depo : IRateCurveInstrument
    {
        private readonly Currency currency;
        private readonly Tenor tenor;
        private readonly double simpleRate;

        public Depo(Currency currency, Tenor tenor, double simpleRate)
        {
            this.currency = currency;
            this.tenor = tenor;
            this.simpleRate = simpleRate;
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
            var curve = marketData.Get(new DiscountingSourceDescription(currency));
            return 0.0;
        }
    }

    public class RateCurveCalibrator
    {

    }
}
