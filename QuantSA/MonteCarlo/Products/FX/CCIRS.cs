using QuantSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
    public class CCIRS  :Product
    {
        //Receive Leg
        Currency currency1;
        MarketObservable floatingIndex1;
        Date[] indexDates1;
        Date[] payDates1;
        double[] spread1;
        double[] accrualFractions1;

        //Pay Leg
        Currency currency2;
        MarketObservable floatingIndex2;
        Date[] indexDates2;
        Date[] payDates2;
        double[] spread2;
        double[] accrualFractions2;

        // Product state
        Date valueDate;
        double[] indexValues1;
        double[] indexValues2;

        public override void SetValueDate(Date valueDate)
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            throw new NotImplementedException();
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            throw new NotImplementedException();
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            throw new NotImplementedException();
        }

        public override List<Cashflow> GetCFs()
        {
            throw new NotImplementedException();
        }
    }
}
