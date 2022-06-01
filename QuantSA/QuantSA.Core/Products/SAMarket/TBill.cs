using System;
using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.SAMarket
{
    /// <summary>
    /// SA T-bill.
    /// </summary>
    public class TBill : ProductWrapper
    {
        public Date maturityDate;
        private readonly List<Cashflow> _cfs;

        public override List<Cashflow> GetCFs()
        {
            return _cfs;
        }
    }
}