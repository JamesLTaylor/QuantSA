using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation
{
    /// <summary>
    /// Base class for bank account numeraire simulators 
    /// </summary>
    public abstract class NumeraireSimulator : Simulator
    {
        /// <summary>
        /// Get the numeraire currency.  All cashflows will be converted to this currency and discounted
        /// under each simulation at the simulated bank account.
        /// </summary>
        /// <returns></returns>
        public abstract Currency GetNumeraireCurrency();

        /// <summary>
        /// Get the numeraire within a simulation at the provided date.
        /// </summary>
        /// <param name="valueDate">The date on which the Numeraire is required.</param>
        /// <returns></returns>
        public abstract double Numeraire(Date valueDate);

        /// <summary>
        /// Sets dates on which it is expected that the numeraire will be required.
        /// </summary>
        /// <param name="requiredDates">The required dates.</param>
        public abstract void SetNumeraireDates(List<Date> requiredDates);
    }
}