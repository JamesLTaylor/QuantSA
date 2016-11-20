using System;
using System.Collections.Generic;
using QuantSA.General;

namespace QuantSA.Valuation
{
    //TODO: This class must be extended to also handle curves.
    [Serializable]
    public class DeterministicNumeraire : NumeraireSimulator
    {
        private Currency currency;
        private Date anchorDate;
        private SingleRate singleRate;

        /// <summary>
        /// Create the simplest <see cref="NumeraireSimulator"/>.  Uses a single continuous rate and does no simulation.
        /// </summary>
        /// <param name="currency">The currency of the bank numeraire</param>
        /// <param name="anchorDate">The time at which the numeraire will be 1.</param>
        /// <param name="rate">The single continuous rsate that will be used for all discounting.</param>
        public DeterministicNumeraire(Currency currency, Date anchorDate, double rate)
        {
            this.currency = currency;
            this.anchorDate = anchorDate;
            singleRate = SingleRate.Continuous(rate, anchorDate);
        }

        public override double Numeraire(Date valueDate)
        {
            return 1 / singleRate.GetDF(valueDate);
        }

        public override Currency GetNumeraireCurrency()
        {
            return currency;
        }

        public override void RunSimulation(int i)
        {
            // Do nothing.
        }

        
        public override bool ProvidesIndex(MarketObservable index)
        {
            return false; // Provides no MarketObservables.
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            // Do nothing, it has no state
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            throw new NotImplementedException("This method should be never be called.  The simulator provides no MarketObservables.");
        }

        public override void Reset()
        {
            // Do nothing, it has no state.
        }

        public override void Prepare()
        {
            // Do nothing, it has no state.
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            // Do nothing, it has no state.
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            return new double[] { 1 };
        }
    }
}
