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
        public static ResultStore BlackOption(JSEBondOption bondoption, double strike, double vol, double repo, JSEBondForward bond, double yieldToMaturity)
        {
            var settleDate = bondoption.settleDate;
            var timeToMaturity = bondoption.timeToMaturity;
            var ytm = yieldToMaturity;
            var bondforwardprice1 = (double) bond.ForwardPrice(settleDate, ytm, repo).GetScalar(JSEBondForwardEx.Keys.ForwardPrice);

            var discountFactor = Math.Exp(-repo * timeToMaturity);

            var optionPrice = BlackEtc.Black(PutOrCall.Call, strike, timeToMaturity, bondforwardprice1, vol, discountFactor);

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
