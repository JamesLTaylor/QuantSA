using QuantSA;
using System;
using System.Collections.Generic;

namespace MonteCarlo
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
        /// <param name="valueDate"></param>
        /// <returns></returns>
        public abstract double Numeraire(Date valueDate);

        public abstract void SetNumeraireDates(List<Date> requiredDates);

    }
}