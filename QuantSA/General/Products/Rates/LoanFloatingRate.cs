using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A Floating rate loan with exactly the same structure as <see cref="FloatLeg"/> but with notional flows added.
    /// </summary>    
    /// <seealso cref="QuantSA.General.FloatLeg" />
    /// <seealso cref="QuantSA.General.IProvidesResultStore" />
    public class LoanFloatingRate : FloatLeg, IProvidesResultStore
    {
        //TODO: Move float leg to a component rather than a base class.  See https://en.wikipedia.org/wiki/Composition_over_inheritance
        private double spread;
        private FloatingIndex index;
        private List<Cashflow> notionalFlows;

        public LoanFloatingRate() : base() { }

        /// <summary>
        /// Create a floating rate loan from a loan profile.  The first date in the profile is the disbursment date and 
        /// the last date is the final repayment date.  Interest will due on all but the first profile date.
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="balanceDates">A date must be given for each interest date and this original disbursement date
        /// even if the balances remain constant.</param>
        /// <param name="simpleFixedRate">Interest will be calculated simple </param>
        /// <returns></returns>
        public static LoanFloatingRate CreateSimple(Date[] balanceDates, double[] balances, FloatingIndex index,  
            double spread, Currency ccy)
        {
            LoanFloatingRate loan = new LoanFloatingRate();
            loan.valueDate = null;
            loan.ccy = ccy;
            loan.spread = spread;
            loan.notionalFlows = new List<Cashflow>();
            loan.notionalFlows.Add(new Cashflow(balanceDates[0], -balances[0], ccy));

            // Set the details for the FloatLeg
            loan.paymentDates = new Date[balanceDates.Length - 1];
            loan.resetDates = new Date[balanceDates.Length - 1];
            loan.floatingIndices = new FloatingIndex[balanceDates.Length - 1];
            loan.spreads = new double[balanceDates.Length - 1];
            loan.notionals = new double[balanceDates.Length - 1]; 
            loan.accrualFractions = new double[balanceDates.Length - 1];

            for (int i = 1; i < balances.Length; i++)
            {
                loan.paymentDates[i - 1] = balanceDates[i];
                loan.resetDates[i - 1] = balanceDates[i-1];
                loan.floatingIndices[i - 1] = index;
                loan.spreads[i - 1] = spread;
                loan.notionals[i - 1] = balances[i-1];
                loan.accrualFractions[i - 1] = (balanceDates[i] - balanceDates[i - 1]) / 365.0;

                double notionalFlow = balances[i - 1] - balances[i];
                loan.notionalFlows.Add(new Cashflow(balanceDates[i], notionalFlow, ccy));
            }

            loan.type = "LoanFloatingRate";
            loan.index = index;
            return loan;
        }

        /// <summary>
        /// overrides the GetCFs in FloatLeg to include the notional flows in the loan.
        /// </summary>
        /// <returns></returns>
        public override List<Cashflow> GetCFs()
        {
            List<Cashflow> cfs = base.GetCFs();
            cfs.AddRange(notionalFlows);            
            return cfs;
        }


        /// <summary>
        /// Some useful information for a user who has an instance of this class.
        /// </summary>
        /// <returns></returns>
        public ResultStore GetResultStore()
        {
            ResultStore results = new ResultStore();
            results.Add("id", id);
            results.Add("type", type);
            results.Add("ccy", ccy.ToString());
            results.Add("floatIndex", index.ToString());
            results.Add("spread", spread);
            results.Add("loanDates", resetDates);
            results.Add("loanBalances", notionals);
            return results;
        }
    }
}
