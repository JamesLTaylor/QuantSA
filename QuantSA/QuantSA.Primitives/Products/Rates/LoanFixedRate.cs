using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    /// <summary>    
    /// </summary>
    /// <remarks>
    /// This fixed rate loan has been implemented as a CashLeg with a convenient constructor.
    /// </remarks>
    /// <seealso cref="QuantSA.General.CashLeg" />
    /// <seealso cref="QuantSA.General.IProvidesResultStore" />
    [Serializable]
    public class LoanFixedRate : CashLeg, IProvidesResultStore
    {
        private Currency ccy;
        private double fixedRate;
        private Date[] balanceDates;
        private double[] balanceAmounts;


        public LoanFixedRate() : base() { }
        

        /// <summary>
        /// Create a fixed rate loan from a loan profile.  The first date in the profile is the disbursment date and 
        /// the last date is the final repayment date.  Interest will due on all but the first profile date.
        /// 
        /// </summary>
        /// <param name="balanceDates">A date must be given for each interest date and this original disbursement date
        /// even if the balances remain constant.</param>
        /// <param name="simpleFixedRate">Interest will be calculated simple </param>
        /// <returns></returns>
        public static LoanFixedRate CreateSimple(Date[] balanceDates, double[] balanceAmounts, double simpleFixedRate, Currency ccy)
        {            
            LoanFixedRate loan = new LoanFixedRate();
            loan.balanceAmounts = balanceAmounts;
            loan.balanceDates = balanceDates;
            loan.valueDate = null;
            loan.ccy = ccy;
            loan.fixedRate = simpleFixedRate;
            loan.cfs = new List<Cashflow>();
            loan.cfs.Add(new Cashflow(balanceDates[0], -balanceAmounts[0], ccy));

            for (int i = 1; i < balanceAmounts.Length; i++)
            {
                double notional = balanceAmounts[i - 1] - balanceAmounts[i];
                double interest = balanceAmounts[i - 1] * simpleFixedRate * (balanceDates[i] - balanceDates[i - 1]) / 365.0;
                loan.cfs.Add(new Cashflow(balanceDates[i], notional+interest, ccy));
            }
            loan.type = "LoanFixedRate";
            return loan;
        }

        public ResultStore GetResultStore()
        {
            ResultStore results = new ResultStore();
            results.Add("id", id);
            results.Add("type", type);
            results.Add("ccy", ccy.ToString());
            results.Add("fixedRate", fixedRate);
            results.Add("loanDates", balanceDates);
            results.Add("loanBalances", balanceAmounts);
            return results;
        }
    }
}
