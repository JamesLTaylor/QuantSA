using System;
using System.Collections.Generic;
using System.Text;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using QuantSA.Core.Primitives;
using QuantSA.Shared.MarketObservables;
using QuantSA.Core.Formulae;


namespace QuantSA.Core.Products.SAMarket
{
    public class JSEBondOption : ProductWrapper
    {
        /// <summary>
        /// Bond forward date
        /// </summary>
        public Date forwardDate;

        public Date settleDate;

        public PutOrCall putOrCall;

        public double timeToMaturity;

        public JSEBondOption(Date forwardDate, Date maturityDate, PutOrCall putOrCall, Date settleDate)
        {
            if (forwardDate > maturityDate)
                throw new ArgumentException("forward date must be before maturity date.");

            this.forwardDate = forwardDate;
            this.putOrCall = putOrCall;
            this.settleDate = settleDate;
            timeToMaturity = (double)(forwardDate - settleDate) / 365;
        }

        public override List<Cashflow> GetCFs()
        {
            throw new NotImplementedException();
        }
    }
}
