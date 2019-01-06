using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;

namespace QuantSA.CoreExtensions.Curves
{
    public interface IRateCurveInstrument
    {
        string GetName();

        void SetCalibrationDate(Date calibrationDate);

        /// <summary>
        /// Get the curve for which the initial value applies and the date and rate it should have.
        /// </summary>
        /// <returns></returns>
        Tuple<string, Date, double> GetInitialValue();

        void SetMarketData(IMarketDataContainer marketData);

        double Objective();
    }
}
