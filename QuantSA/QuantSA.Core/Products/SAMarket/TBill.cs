using System;
using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.SAMarket
{
    /// <summary>
    /// The market standard bond traded on the JSE, formerly the BESA.
    /// </summary>
    public class TBill : ProductWrapper
    {
        private readonly List<Cashflow> _cfs;

        public override List<Cashflow> GetCFs()
        {
            return _cfs;
        }
    }
}