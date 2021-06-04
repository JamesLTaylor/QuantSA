using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    public abstract class AssetSwap : Product
    {

        private readonly FixedLeg _fixedLeg;
        private readonly FloatLeg _floatLeg;
        private readonly Date[] _indexDates;
        private readonly double[] _nominal;
        private readonly Date[] _paymentDates;
        private readonly double[] _spreads;
        private readonly double _couponrate;
        private readonly double[] Bondprice;

        [JsonIgnore] private List<Date> _futureIndexDates;
        [JsonIgnore] private List<Date> _futurePayDates;

        // Product state
        [JsonIgnore] private double[] _indexValues;
        [JsonIgnore] private Date _valueDate;

        /// <param name="couponrate"></param>
        /// <param name="fixedleg"></param>
        /// <param name="accrualFractions"></param>
        /// <param name="notionals"></param>
        /// <param name="fixedRate"></param>

        public AssetSwap(FixedLeg _fixedLeg, FloatLeg _floatLeg, double[] nominal, double couponrate, double[] Bondprice)
        {
            //initiate the variables
          _nominal = nominal;
          _couponrate = couponrate;
            _fixedLeg = new FixedLeg();
            _fixedLeg.GetCFs();
          _floatLeg = new FloatLeg();
          Bondprice = new Bondprice();
         

          var spreads = (Bondprice - nominal + _fixedLeg - _floatLeg) / (_fixedLeg / couponrate);
          return spreads;

        }
    }
}
