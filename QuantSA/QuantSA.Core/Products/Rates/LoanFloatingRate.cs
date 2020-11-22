using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    /// <summary>
    /// A Floating rate loan with exactly the same structure as <see cref="FloatLeg" /> but with notional flows added.
    /// </summary>
    /// <seealso cref="FloatLeg" />
    public class LoanFloatingRate : FloatLeg
    {
        private FloatRateIndex _index;

        private List<Cashflow> _notionalFlows;

        //TODO: Move float leg to a component rather than a base class.  See https://en.wikipedia.org/wiki/Composition_over_inheritance
        private double _spread;

        /// <summary>
        /// Create a floating rate loan from a loan profile.  The first date in the profile is the disbursment date and
        /// the last date is the final repayment date.  Interest will due on all but the first profile date.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="balanceDates">
        /// A date must be given for each interest date and this original disbursement date
        /// even if the balances remain constant.
        /// </param>
        /// <param name="balances">This array must be the same length as <paramref name="balanceDates"/>.</param>
        /// <param name="index">The rate that the interest rate payments will be based off.</param>
        /// <param name="spread"></param>
        /// <param name="ccy"></param>
        /// <returns></returns>
        public static LoanFloatingRate CreateSimple(Date[] balanceDates, double[] balances, FloatRateIndex index,
            double spread, Currency ccy)
        {
            var loan = new LoanFloatingRate();
            loan.valueDate = null;
            loan.ccy = ccy;
            loan._spread = spread;
            loan._notionalFlows = new List<Cashflow>();
            loan._notionalFlows.Add(new Cashflow(balanceDates[0], -balances[0], ccy));

            // Set the details for the FloatLeg
            loan.paymentDates = new Date[balanceDates.Length - 1];
            loan.resetDates = new Date[balanceDates.Length - 1];
            loan.floatingIndices = new FloatRateIndex[balanceDates.Length - 1];
            loan.spreads = new double[balanceDates.Length - 1];
            loan.notionals = new double[balanceDates.Length - 1];
            loan.accrualFractions = new double[balanceDates.Length - 1];

            for (var i = 1; i < balances.Length; i++)
            {
                loan.paymentDates[i - 1] = balanceDates[i];
                loan.resetDates[i - 1] = balanceDates[i - 1];
                loan.floatingIndices[i - 1] = index;
                loan.spreads[i - 1] = spread;
                loan.notionals[i - 1] = balances[i - 1];
                loan.accrualFractions[i - 1] = (balanceDates[i] - balanceDates[i - 1]) / 365.0;

                var notionalFlow = balances[i - 1] - balances[i];
                loan._notionalFlows.Add(new Cashflow(balanceDates[i], notionalFlow, ccy));
            }

            loan.Type = "LoanFloatingRate";
            loan._index = index;
            return loan;
        }

        /// <summary>
        /// overrides the GetCFs in FloatLeg to include the notional flows in the loan.
        /// </summary>
        /// <returns></returns>
        public override List<Cashflow> GetCFs()
        {
            var cfs = base.GetCFs();
            foreach (var flow in _notionalFlows)
                if (flow.Date > valueDate)
                    cfs.Add(flow);
            return cfs;
        }
    }
}