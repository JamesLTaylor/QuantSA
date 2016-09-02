using QuantSA;
using System;

namespace MonteCarlo
{
    /// <summary>
    /// Base class for bank account numeraire simulators 
    /// </summary>
    public abstract class NumeraireSimulator
    {
        public abstract Currency GetCurrency();
        public abstract void RunSimulation(int i);
        public abstract double At(Date valueDate);
    }
}