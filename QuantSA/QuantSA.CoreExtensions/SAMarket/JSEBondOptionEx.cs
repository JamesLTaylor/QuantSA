using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QuantSA.Core.Products.SAMarket;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Core.Formulae;
using MathNet.Numerics.Distributions;



namespace QuantSA.CoreExtensions.SAMarket
{
    public static class JSEBondOptionEx
    {
        public static ResultStore BlackOption(JSEBondForward bondforward, PutOrCall putOrCall, double strike, double vol, double bondforwardprice, double repo)
        {
            var dist = new Normal();
            var forwardDate = bondforward.forwardDate;
            var settleDate = bondforward.settleDate;
            var timeToMaturity = (double)(forwardDate - settleDate) / 365;
            var sigmaSqrtT = vol * Math.Sqrt(timeToMaturity);
            var d1 = 1 / sigmaSqrtT * (Math.Log(bondforwardprice / strike) + 0.5 * vol * vol);
            var d2 = d1 - sigmaSqrtT;


            var discountFactor = Math.Exp(-repo * timeToMaturity);

            var optionPrice = (double)putOrCall * discountFactor * (bondforwardprice * dist.CumulativeDistribution(d1) - strike * dist.CumulativeDistribution(d2));
            var resultStore = new ResultStore();
            resultStore.Add(Keys.BlackOption, optionPrice);
            return resultStore;
        }

        public static class Keys
        {
            public const string BlackOption = "optionPrice";
        }
    }
}
