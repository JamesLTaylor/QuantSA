using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;

namespace QuantSA.Valuation.Models
{
    public class MultiHWAndFXToy : NumeraireSimulator
    {
        private MultivariateNormalDistribution normal;
        private Currency numeraireCcy;
        private HullWhite1F numeraireSimulator;
        private HullWhite1F[] rateSimulators;
        private CurrencyPair[] currencyPairs;
        private double[] spots;
        private double[] vols;
        private Date anchorDate;

        private Dictionary<Currency, HullWhite1F> ccySimMap;

        private List<Date> allRequiredDates; // the set of all dates that will be simulated.
        private Dictionary<int, double[]> simulation; // stores the simulated spot rates at each required date


        public MultiHWAndFXToy(Date anchorDate, Currency numeraireCcy, HullWhite1F[] rateSimulators, CurrencyPair[] currencyPairs, 
            double[] spots, double[] vols, double[,] correlations)
        {
            this.anchorDate = anchorDate;
            this.numeraireCcy = numeraireCcy;
            this.rateSimulators = rateSimulators;
            this.currencyPairs = currencyPairs;
            this.spots = spots;
            this.vols = vols;
            normal = new MultivariateNormalDistribution(Vector.Zeros(currencyPairs.Length), correlations);
            numeraireSimulator = null;
            ccySimMap = new Dictionary<Currency, HullWhite1F>();
            foreach (HullWhite1F simulator in rateSimulators)
            {
                if (simulator.GetNumeraireCurrency() == numeraireCcy)
                {
                    numeraireSimulator = simulator;
                }
                ccySimMap[simulator.GetNumeraireCurrency()] = simulator;
            }
            if (numeraireSimulator == null) throw new ArgumentException("A rate simulator must be provided for the numeraire currency: " + numeraireCcy.ToString());

        }
            

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            if (index is FloatingIndex)
            {
                foreach (HullWhite1F rateSimulator in rateSimulators)
                {
                    if (rateSimulator.ProvidesIndex(index)) return rateSimulator.GetIndices(index, requiredDates);
                }
                throw new ArgumentException(index.ToString() + " is not provided by any of the simulators.");
            }
            if (index is CurrencyPair)
            {
                int currencyPairIndex = currencyPairs.IndexOf(index);
                double[] result = new double[requiredDates.Count];
                for (int i = 0; i < requiredDates.Count; i++)
                {
                    if (requiredDates[i] == anchorDate)
                        result[i] = spots[currencyPairIndex];
                    else
                        result[i] = simulation[requiredDates[i]][currencyPairIndex];
                }
                return result;
            }
            throw new ArgumentException(index.ToString() + " is not provided by this model.");
        }

        public override Currency GetNumeraireCurrency()
        {
            return numeraireCcy;
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            double[] factors = new double[currencyPairs.Length + rateSimulators.Length];
            for (int i = 0; i < currencyPairs.Length; i++)
            {
                factors[i] = GetIndices(currencyPairs[i], new List<Date> { date })[0];
            }
            for (int i = 0; i < rateSimulators.Length; i++)
            {
                factors[i + currencyPairs.Length] = rateSimulators[i].GetUnderlyingFactors(date)[0];
            }
            return factors;
        }
        
    

        public override double Numeraire(Date valueDate)
        {
            return numeraireSimulator.Numeraire(valueDate);
        }

        public override void Prepare()
        {
            foreach (HullWhite1F simulator in rateSimulators)
                simulator.Prepare();
            allRequiredDates = allRequiredDates.Distinct().ToList();
            allRequiredDates.Sort();
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            foreach (HullWhite1F simulator in rateSimulators)
            {
                if (simulator.ProvidesIndex(index))
                    return true;
            }
            if (currencyPairs.Contains(index))
                return true;
            return false;
        }

        public override void Reset()
        {
            foreach (HullWhite1F simulator in rateSimulators)
                simulator.Reset();
            allRequiredDates = new List<Date>();
        }

        public override void RunSimulation(int simNumber)
        {
            foreach (HullWhite1F simulator in rateSimulators)
                simulator.RunSimulation(simNumber);
            simulation = new Dictionary<int, double[]>();
            double[] simPrices = spots.Copy();
            double[] oldDrifts = Vector.Ones(simPrices.Length);

            for (int timeCounter = 0; timeCounter < allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0 ? allRequiredDates[timeCounter] - allRequiredDates[timeCounter - 1] : allRequiredDates[timeCounter] - anchorDate.value;
                dt = dt / 365.0;
                double sdt = Math.Sqrt(dt);
                double[] dW = normal.Generate();

                for (int s = 0; s < currencyPairs.Length; s++)
                {
                    double drift = ccySimMap[currencyPairs[s].counterCurrency].Numeraire(allRequiredDates[timeCounter]) /
                        ccySimMap[currencyPairs[s].baseCurrency].Numeraire(allRequiredDates[timeCounter]);
                    
                    simPrices[s] = simPrices[s] * drift / oldDrifts[s] *
                        Math.Exp(( - 0.5 * vols[s] * vols[s]) * dt + vols[s] * sdt * dW[s]);
                    oldDrifts[s] = drift;
                }
                simulation[allRequiredDates[timeCounter]] = simPrices.Copy();
            }

        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            foreach (HullWhite1F simulator in rateSimulators)
                simulator.SetNumeraireDates(requiredDates);
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            foreach (HullWhite1F simulator in rateSimulators)
            {
                if (simulator.ProvidesIndex(index)) simulator.SetRequiredDates(index, requiredDates);              
            }
            if (index is CurrencyPair)
                allRequiredDates.AddRange(requiredDates);
        }
    }
}
