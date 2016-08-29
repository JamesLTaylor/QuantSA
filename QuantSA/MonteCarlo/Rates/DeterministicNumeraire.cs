using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General;
using General.Rates;

namespace MonteCarlo.Rates
{
    //TODO: This class must be extended to also handle curves.
    public class DeterministicNumeraire : NumeraireSimulator
    {
        private Currency ccy;
        private Date anchorDate;
        private SingleRate singleRate;

        /// <summary>
        /// Create the simplest <see cref="NumeraireSimulator"/>.  Uses a single continuous rate and does no simulation.
        /// </summary>
        /// <param name="ccy">The currency of the bank numeraire</param>
        /// <param name="anchorDate">The time at which the numeraire will be 1.</param>
        /// <param name="rate">The single continuous rsate that will be used for all discounting.</param>
        public DeterministicNumeraire(Currency ccy, Date anchorDate, double rate)
        {
            this.ccy = ccy;
            this.anchorDate = anchorDate;
            singleRate = SingleRate.Continuous(rate, anchorDate);
        }

        public override double At(Date valueDate)
        {
            return 1 / singleRate.GetDF(valueDate);
        }

        public override Currency GetCurrency()
        {
            return ccy;
        }

        public override void RunSimulation(int i)
        {
            // Do nothing.
        }
    }
}
