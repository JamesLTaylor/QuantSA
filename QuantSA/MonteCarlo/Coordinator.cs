using QuantSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
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
        /// <param name="portfolio"></param>
        /// <param name="simulators">Optionally any extra simulators independent of the first.  Can be an empty list.</param>
        public Coordinator(NumeraireSimulator numeraire, List<Product> portfolio, List<Simulator> simulators, int N)
        {
            this.numeraire = numeraire;
            this.portfolio = portfolio;
            this.simulators = simulators;
            this.simulators.Insert(0, numeraire);
            this.N = N;
        }

        public double Value(Date valueDate)
        {
            Dictionary<Product, Dictionary< MarketObservable, Simulator >> indexSources = new Dictionary<Product, Dictionary<MarketObservable, Simulator>>();
            foreach (Product product in portfolio)
            {
                Dictionary<MarketObservable, Simulator> indicesAndSources = new Dictionary<MarketObservable, Simulator>();
                foreach (MarketObservable index in product.GetRequiredIndices()){
                    bool found = false;
                    foreach (Simulator simulator in simulators)
                    {
                        if (simulator.ProvidesIndex(index))
                        {
                            indicesAndSources[index] = simulator;
                            found = true;
                            break;
                        }
                    }
                    if (!found) throw new IndexOutOfRangeException("Required index: " + index.ToString() + " is not provided by any of the simulators");
                    indexSources[product] = indicesAndSources;
                }
            }

            // Reset all the simulators
            foreach (Simulator simulator in simulators)
            { simulator.Reset(); }

            // Set up the simulators for the times at which they will be queried
            foreach (Product product in portfolio)
            {                
                foreach (KeyValuePair < MarketObservable, Simulator> entry in indexSources[product])
                {
                    product.SetValueDate(valueDate);
                    List<Date> requiredTimes = product.GetRequiredIndexDates(entry.Key);
                    entry.Value.SetRequiredTimes(entry.Key, requiredTimes);
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
                    foreach (MarketObservable index in product.GetRequiredIndices())
                    {
                        Simulator simulator = indexSources[product][index];
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
                            throw new Exception("No simulator to convert from " + cf.currency.ToString() + " to the valuation currency: " + valueCurrency.ToString());
                        }
                    }
                }
                totalValue += pathwiseValues[i];
            }
            return totalValue/N;

        }
    }
}
