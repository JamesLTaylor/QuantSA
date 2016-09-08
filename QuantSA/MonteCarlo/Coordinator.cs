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
        private NumeraireSimulator numeraire;
        private List<Product> portfolio;
        private List<Simulator> simulators;

        public Coordinator(NumeraireSimulator numeraire, List<Product> portfolio, List<Simulator> simulators)
        {
            this.numeraire = numeraire;
            this.portfolio = portfolio;
            this.simulators = simulators;
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
                    int[] requiredTimes = product.GetRequiredTimes(valueDate, entry.Key);
                    entry.Value.SetRequiredTimes(entry.Key, requiredTimes);
                }
            }

            // Run the simulation
            Currency valueCurrency = numeraire.GetCurrency();            
            int N = 100000;
            // TODO: Rather store value for each product separately
            double[] pathwiseValues = new double[N];
            double totalValue = 0;
            for (int i=0; i< N; i++)
            {
                pathwiseValues[i] = 0;
                numeraire.RunSimulation(i);
                double numeraireAtValue = numeraire.At(valueDate);
                foreach (Simulator simulator in simulators)
                {
                    simulator.RunSimulation(i);
                }
                foreach (Product product in portfolio)
                {
                    foreach (MarketObservable index in product.GetRequiredIndices())
                    {
                        Simulator simulator = indexSources[product][index];
                        int[] requiredTimes = product.GetRequiredTimes(valueDate, index);
                        double[] indices = simulator.GetIndices(index, requiredTimes);
                        product.SetIndices(index, indices);                        
                    }
                    // TODO: Generate cashflows per currency and then convert before using the numeraire for discounting
                    double[,] timesAndCFS = product.GetCFs();
                    for (int cfCounter = 0; cfCounter<timesAndCFS.GetLength(0); cfCounter++)
                    {
                        pathwiseValues[i] += timesAndCFS[cfCounter, 1] * numeraireAtValue / numeraire.At(new Date(timesAndCFS[cfCounter, 0]));
                    }
                }
                totalValue += pathwiseValues[i];
            }
            return totalValue/N;

        }
    }
}
