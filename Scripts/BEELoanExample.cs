Date dealStartDate = new Date(2016, 1, 1); // The issue date of the scheme
Date dealEndDate = dealStartDate.AddTenor(Tenor.Years(10)); // The date on which solvency will be checked.       
double nShares = 1; // Simulator must be set up so the spot value of the equity is the value of the equity held by the SPV
Share share = new Share("AAA", Currency.ZAR);
Dividend dividend = new Dividend(new Share("AAA", Currency.ZAR)); // Used for getting the dividend from the model
FloatingIndex jibar = FloatingIndex.JIBAR3M; // Used for getting the floating interest rate on the loan.

public override List<Cashflow> GetCFs()
{
    double loanBalance = 100; // Opening balance on startDate
    double spread = 0.035 + 0.0185; // 350bp prime Jibar spread + 185bp deal spread

    Date periodStartDate = dealStartDate;
    Date periodEndDate = dealStartDate.AddMonths(3); // These two dates form the interest period over which interest is 
                                                     // charged and dividends are applied as repayments.
    
    List<Cashflow> cfs = new List<Cashflow>(); // an empty set of cashflows that will be added to.
    
    // Generate the dividend cashflows and update the loan balances.
    while (periodEndDate<=dealEndDate)
    {
        double observedRate = Get(jibar, periodStartDate); // Get the simulated interest rate at the period start date
        if (loanBalance < 1e-6)  // If the loan is repaid then no further dividends flow back to issuing company
        {
            cfs.Add(new Cashflow(new Date(periodEndDate), 0, Currency.ZAR));
        }
        else
        {
            loanBalance *= (1 + (observedRate + spread) * 0.25); // The loan accrues interest
            double loanReduction = 0.85 * nShares * Get(dividend, periodEndDate); // 85% of the dividend is available to repay the loan
            if (loanReduction > loanBalance) // Is the dividend greater than the amount still owed on the loan.                
                cfs.Add(new Cashflow(new Date(periodEndDate), loanBalance, Currency.ZAR));
            else
                cfs.Add(new Cashflow(new Date(periodEndDate), loanReduction, Currency.ZAR));
            loanBalance = Math.Max(0, loanBalance - loanReduction); // The loan balance can never drop below zero.
        }
        periodStartDate = new Date(periodEndDate);
        periodEndDate = periodEndDate.AddMonths(3); // Update the dates used for interest and dividend calculations
    }
    // At the final date check if the loan is worth more than the shares.  If yes then there is a loss on the loan, 
    // otherwise the loan can be repaid in full.
    double finalPayment = Math.Min(loanBalance, nShares * Get(share, dealEndDate));
    cfs.Add(new Cashflow(new Date(dealEndDate), finalPayment, Currency.ZAR));

    return cfs;
}

