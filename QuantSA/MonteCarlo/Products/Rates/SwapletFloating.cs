using MonteCarlo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    public class SwapletFloating : Product
    {
        // Product specs            
        Date indexDate;
        Date payDate;
        MarketObservable index;
        double spread;
        double accrualFraction;
        double notional;
        Currency ccy;
        // Product state
        Date valueDate;
        double indexValue;

        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
        }

        public override void Reset()
        {
            // indexValue will just be overwritten.
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            if (valueDate < payDate) return new List<MarketObservable>() { index };
            else return new List<MarketObservable>();
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            if (valueDate < payDate) return new List<Date>() { valueDate };
            else return new List<Date>();
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            this.indexValue = indexValues[0];
        }

        public override List<Cashflow> GetCFs()
        {
            double cf = notional * accrualFraction * (indexValue + spread);
            if (valueDate < payDate) return new List<Cashflow>() { new Cashflow(payDate, cf, ccy) };
            else return new List<Cashflow>();
        }
    }
}
