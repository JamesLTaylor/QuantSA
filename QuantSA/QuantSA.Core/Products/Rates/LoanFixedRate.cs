using System;
using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    /// <summary>    
    /// </summary>
    /// <remarks>
    /// This fixed rate loan has been implemented as a CashLeg with a convenient constructor.
    /// </remarks>
    /// <seealso cref="QuantSA.General.CashLeg" />
    [Serializable]
    public class LoanFixedRate : CashLeg
    {
        private double[] balanceAmounts;
        private Date[] balanceDates;
        private Currency ccy;
        private double fixedRate;

        /// <summary>
        /// Create a fixed rate loan from a loan profile.  The first date in the profile is the disbursment date and 
        /// the last date is the final repayment date.  Interest will due on all but the first profile date.
        /// 
        /// </summary>
        /// <param name="balanceDates">A date must be given for each interest date and this original disbursement date
        /// even if the balances remain constant.</param>
        /// <param name="simpleFixedRate">Interest will be calculated simple </param>
        /// <returns></returns>
        public static LoanFixedRate CreateSimple(Date[] balanceDates, double[] balanceAmounts, double simpleFixedRate,
            Currency ccy)
        {
            var loan = new LoanFixedRate();
            loan.balanceAmounts = balanceAmounts;
            loan.balanceDates = balanceDates;
            loan.valueDate = null;
            loan.ccy = ccy;
            loan.fixedRate = simpleFixedRate;
            loan.cfs = new List<Cashflow>();
            loan.cfs.Add(new Cashflow(balanceDates[0], -balanceAmounts[0], ccy));

            for (var i = 1; i < balanceAmounts.Length; i++)
            {
                var notional = balanceAmounts[i - 1] - balanceAmounts[i];
                var interest = balanceAmounts[i - 1] * simpleFixedRate * (balanceDates[i] - balanceDates[i - 1]) /
                               365.0;
                loan.cfs.Add(new Cashflow(balanceDates[i], notional + interest, ccy));
            }

            loan.type = "LoanFixedRate";
            return loan;
        }
    }
}