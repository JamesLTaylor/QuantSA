using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General.Dates;

namespace QuantSA.General
{
    public class Swap
    {
        double[] notionals;
        Date[] resetDates;
        Date[] paymentDates;
        Date[] accrualFractions;
        int floatingTenorInMonths;

        public Swap(double[] notionals, Date[] resetDates, Date[] paymentDates, Date[] accrualFractions, int floatingTenorInMonths)
        {
            this.notionals = notionals;
            this.paymentDates = paymentDates;
            this.resetDates = resetDates;
            this.accrualFractions = accrualFractions;
            this.floatingTenorInMonths = floatingTenorInMonths;
        }

        public double PV(Date valueDate, IDiscountingSource discountingCurve, IFloatingRateSource forecastingCurve)
        {
            return 0.0;
        }
    }

    public class Stripper
    {
        public static void ZARSwapCurve(double jibar, string[] fraDescriptions, double[] fraRates, int[] swapTenors, double[] swapRates)
        {
            // Create FRAs and swaps
        }

        


    }
}
