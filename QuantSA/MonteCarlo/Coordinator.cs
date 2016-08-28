using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
    public class Coordinator
    {
        private List<Product> portfolio;
        private List<Simulator> simulators;

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
                }
            }

            // Set up the simulators
            foreach (Product product in portfolio)
            {                
                foreach (KeyValuePair < MarketObservable, Simulator> entry in indexSources[product])
                {
                    double[] requiredTimes = product.GetRequiredTimes(valueDate, entry.Key);
                    entry.Value.SetRequiredTimes(entry.Key, requiredTimes);
                }
            }
            int N = 10;
            for (int i=0; i< N; i++)
            {
                foreach (Simulator simulator in simulators)
                {
                    simulator.RunSimulation(i, i + 1);
                }
                foreach (Product product in portfolio)
                {
                    foreach (MarketObservable index in product.GetRequiredIndices())
                    {
                        Simulator simulator = indexSources[product][index];
                        double[] requiredTimes = product.GetRequiredTimes(valueDate, index);
                        double[] indices = simulator.GetIndices(index, requiredTimes);
                        product.SetIndices(index, indices);                        
                    }
                    double[,] timesAndCFS = product.GetCFs();
                }

            }




            return 0.0;

        }
    }
}
