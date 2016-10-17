using QuantSA.General;

namespace PluginDemo
{
    public class PluginDiscount : IDiscountingSource
    {
        private Date anchorDate;
        private double rate;

        public PluginDiscount(Date anchorDate, double rate)
        {
            this.rate = rate;
            this.anchorDate = anchorDate;
        }
        public Date getAnchorDate()
        {
            return anchorDate;
        }

        public Currency GetCurrency()
        {
            return Currency.ANY;
        }

        public double GetDF(Date date)
        {
            return 1 / (1 + rate * (date - anchorDate) / 365.0);
        }
    }
}
