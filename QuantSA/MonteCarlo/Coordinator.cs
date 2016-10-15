using QuantSA;
using QuantSA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
    /// <summary>
    /// A generalized way to perform valuations.  Coordinates between products and models.
    /// </summary>
    /// <remarks>
    /// The sequence for a valuation is:
    /// 1: Obtain the required <see cref="MarketObservable"/>s from each product
    /// 2: Check that there is a simulator for each of these and store them.
    /// 3: Set the value date for each product 
    /// 4: Ask the product at what dates cashflows will take place in each of its currencies.
    ///         Tell the FX simulator it will need rates at these dates
    ///         Tell the numeraire simulator it will need values at each of these dates
    /// 5: Ask the product at what dates it will require each of its MarketObservables
    ///         Tell the MarketObservable simulator it will need rates at each of these dates
    /// 6: Perform a simulation
    ///         Each simulator produces MarketObservables internally
    ///         Each product gets these observables set as if time has passed and these values have indeed been observed.
    ///         The product then uses its contractual definitions to calculate what its cashflows would be under this simulated state of the world
    /// 7: The simulator keeps track of the portfolio's cashflows accross each realization of a state of the world.
    /// </remarks>
    public class Coordinator
    {
        private int N;
        private NumeraireSimulator numeraire;
        private List<Product> portfolio;
        private List<Simulator> simulators;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numeraire">A special simulator that also includes the numeraire. There is only one of these.</param>        
        /// <param name="simulators">Optionally any extra simulators independent of the first.  Can be an empty list.</param>
        public Coordinator(NumeraireSimulator numeraire, List<Simulator> simulators, int N)
        {
            this.numeraire = numeraire;
            this.simulators = simulators;
            this.simulators.Insert(0, numeraire);
            this.N = N;
        }

        public double Value(List<Product> portfolio, Date valueDate)
        {
            this.portfolio = portfolio;
            // Find which simulator will provide each of the potentially required MarketObservables.
            Dictionary<MarketObservable, Simulator> indexSources = new Dictionary<MarketObservable, Simulator>();
            HashSet<Currency> requiredCurrencySet = new HashSet<Currency>();
            foreach (Product product in portfolio)
            {
                // Associate the index simulators
                foreach (MarketObservable index in product.GetRequiredIndices()){
                    if (!indexSources.ContainsKey(index)){
                        bool found = false;
                        foreach (Simulator simulator in simulators)
                        {
                            if (simulator.ProvidesIndex(index))
                            {
                                if (!found)
                                {
                                    indexSources[index] = simulator;
                                    found = true;
                                }
                                else throw new ArgumentException(index.ToString() + " is provided by more than one simulator.");                                
                            }
                        }
                        if (!found) throw new IndexOutOfRangeException("Required index: " + index.ToString() + " is not provided by any of the simulators.");
                        
                    }
                }

                // Associate the currency simulators
                foreach (Currency ccy in product.GetCashflowCurrencies())
                {
                    requiredCurrencySet.Add(ccy);
                    MarketObservable index = new CurrencyPair(ccy, numeraire.GetNumeraireCurrency());
                    if (ccy != numeraire.GetNumeraireCurrency() && !indexSources.ContainsKey(index))
                    {
                        bool found = false;
                        foreach (Simulator simulator in simulators)
                        {
                            if (simulator.ProvidesIndex(index))
                            {
                                indexSources[index] = simulator;
                                found = true;
                                break;
                            }
                        }
                        if (!found) throw new IndexOutOfRangeException("Required currency pair: " + index.ToString() + " is not provided by any of the simulators");

                    }
                }
            }

            // Check how many currencies are required by this portfolio, and ensure that ANY is not used as the 
            // valuation currency if there are more than one.  Comparison needs to be done on HashCode because
            // ANY is equal to any Currency in the comparison overloads.
            if (requiredCurrencySet.Count > 1 && numeraire.GetNumeraireCurrency().GetHashCode() == Currency.ANY.GetHashCode())
                throw new ArgumentException("Cannot use 'ANY' as the valuation currency when the portfolio includes cashflows in multiple currencies.");

            // Reset all the simulators
            foreach (Simulator simulator in simulators)
            { simulator.Reset(); }

            // Set up the simulators for the times at which they will be queried
            foreach (Product product in portfolio)
            {
                product.SetValueDate(valueDate);
                // Tell the simulators at what times indices will be required.
                foreach (MarketObservable index in product.GetRequiredIndices())
                {                    
                    List<Date> requiredTimes = product.GetRequiredIndexDates(index);
                    indexSources[index].SetRequiredDates(index, requiredTimes);
                }
                // Tell the nummeraire simulator at what times it will be required.
                // Tell the FX simulators at what times they will be required.
                foreach (Currency ccy in product.GetCashflowCurrencies())
                {
                    List<Date> requiredDates = product.GetCashflowDates(ccy);
                    numeraire.SetNumeraireDates(requiredDates);
                    if (ccy!=numeraire.GetNumeraireCurrency())
                    {
                        MarketObservable index = new CurrencyPair(ccy, numeraire.GetNumeraireCurrency());
                        indexSources[index].SetRequiredDates(index, requiredDates);
                    }
                }
            }

            // Prepare all the simulators
            foreach (Simulator simulator in simulators)
            { simulator.Prepare(); }

            // Run the simulation
            Currency valueCurrency = numeraire.GetNumeraireCurrency();            
            // TODO: Rather store value for each product separately
            double[] pathwiseValues = new double[N];
            double totalValue = 0;
            for (int i=0; i< N; i++)
            {
                pathwiseValues[i] = 0;
                double numeraireAtValue = numeraire.Numeraire(valueDate);
                foreach (Simulator simulator in simulators)
                {
                    simulator.RunSimulation(i);
                }
                foreach (Product product in portfolio)
                {
                    product.Reset();
                    foreach (MarketObservable index in product.GetRequiredIndices())
                    {
                        Simulator simulator = indexSources[index];
                        List<Date> requiredDates = product.GetRequiredIndexDates(index);
                        double[] indices = simulator.GetIndices(index, requiredDates);
                        product.SetIndexValues(index, indices);                        
                    }
                    List<Cashflow> timesAndCFS = product.GetCFs();
                    foreach (Cashflow cf in timesAndCFS)
                    {
                        if (cf.currency.Equals(valueCurrency))
                        {
                            pathwiseValues[i] += cf.amount * numeraireAtValue / numeraire.Numeraire(cf.date);
                        }
                        else
                        {
                            MarketObservable currencyPair = new CurrencyPair(cf.currency, numeraire.GetNumeraireCurrency());
                            double fxRate = indexSources[currencyPair].GetIndices(currencyPair, new List<Date> { cf.date })[0];
                            pathwiseValues[i] += fxRate * cf.amount * numeraireAtValue / numeraire.Numeraire(cf.date) ;
                        }
                    }
                }
                totalValue += pathwiseValues[i];
            }
            return totalValue/N;

        }
    }
}
