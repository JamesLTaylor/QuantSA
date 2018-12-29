using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Shared.MarketData
{
    public interface ICalibrator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataBeingCalibrated"></param>
        void CalibrateAndUpdate(IMarketDataContainer dataBeingCalibrated);
    }
}
