using Accord.Math;
using Accord.Statistics.Models.Regression.Linear;
using QuantSA.General;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Valuation
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
        Dictionary<MarketObservable, int> indexSources;
        Date valueDate;
        Currency valueCurrency;

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
            valueCurrency = numeraire.GetNumeraireCurrency();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// The simulation phase should produce some set of name,value pairs like: 
        ///     productID:simNumber:date,cfValueInNumeraireCcy        
        /// So that a fully flexible distributed calculation might be possible.
        /// </remarks>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        public double[] EPE(List<Product> portfolio, Date valueDate, List<Date> fwdValueDates)
        {
            double[] epe = Vector.Zeros(fwdValueDates.Count);
            double[,] regressedValues = GetRegressedFwdValues(portfolio, valueDate, fwdValueDates);
            //Debug.WriteToFile(@"c:\dev\temp\regressedValues.csv", regressedValues);            
            for (int row=0; row< regressedValues.GetLength(0); row++)
            {
                for (int col = 0; col< regressedValues.GetLength(1); col++)
                {
                    epe[col] += Math.Max(0, regressedValues[row, col]);
                }
            }
            for (int col = 0; col < regressedValues.GetLength(1); col++)
            {
                epe[col] /= N;
            }            
            return epe;
        }


        /// <summary>
        /// Use regression on the underlying simulated factors to estimate the forward values of the portfolio
        /// discounted to the value date.
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        private double[,] GetRegressedFwdValues(List<Product> portfolio, Date valueDate, List<Date> fwdValueDates)
        {
            this.portfolio = portfolio;
            this.valueDate = valueDate;
            AssociateFactorsWithSimulators();
            InitializeSimulators(fwdValueDates);

            // run one simulation to see how many indepedent variables each simulator provides
            int independentCount = 0;
            foreach (Simulator simulator in simulators)
            {
                simulator.RunSimulation(0);
                independentCount += simulator.GetUnderlyingFactors(fwdValueDates[0]).Length;
            }

            int nTasks = 16;
            double[,] pathwiseCfValues = new double[N, fwdValueDates.Count];
            double[,] regressedValues = new double[N, fwdValueDates.Count];
            double[,,] pathwiseIndependent = new double[N, fwdValueDates.Count, independentCount];

            Debug.StartTimer();
            // Run the simulation
            //PerformSimulationChunk(fwdValueDates, pathwiseCfValues, regressedValues, pathwiseIndependent, 0, N);
            Task[] simTasks = new Task[nTasks];
            int simChunkSize = (int)Math.Ceiling(N / (double)nTasks);
            for (int i = 0; i < nTasks; i++)
            {
                int start = i * simChunkSize;
                int end = Math.Min(start + simChunkSize, N);
                simTasks[i] = Task.Factory.StartNew(() => PerformSimulationChunk(fwdValueDates, pathwiseCfValues, regressedValues, pathwiseIndependent, start, end));
            }
            Task.WaitAll(simTasks);
            double elapsedTime1 = Debug.ElapsedTime();
            Debug.StartTimer();
            //DebugWriteToFile(@"c:\dev\temp\pathwiseCfValues.csv", pathwiseValues);

            // All future cashflows have been found.  Do the regression.
            // normal loop
            //for (int col = 0; col < fwdValueDates.Count; col++)
            //    PerformRegression(pathwiseCfValues, pathwiseIndependent, regressedValues, col);
            // Parrallel loop
            //Parallel.For(0, fwdValueDates.Count,
            //    i => PerformRegression(pathwiseCfValues, pathwiseIndependent, regressedValues, i));
            // Task factory
            
            Task[] regressTasks = new Task[nTasks];
            int regressChunkSize = (int)Math.Ceiling(fwdValueDates.Count() / (double)nTasks);
            for (int i = 0; i < nTasks; i++)
            {
                int start = i * simChunkSize;
                int end = Math.Min(start + simChunkSize, fwdValueDates.Count());
                simTasks[i] = Task.Factory.StartNew(() => PerformRegressionChunk(pathwiseCfValues, pathwiseIndependent, regressedValues, start, end));
            }
            Task.WaitAll(simTasks);
            
            double elapsedTime2 = Debug.ElapsedTime();
            return regressedValues;
        }

        /// <summary>
        /// Performs the regression at a range of times.
        /// </summary>
        private void PerformRegressionChunk(double[,] pathwiseCfValues, double[,,] pathwiseIndependent,
            double[,] regressedValues, int colStart, int colEnd)
        {
            for (int i = colStart; i < colEnd; i++)
                PerformRegression(pathwiseCfValues, pathwiseIndependent, regressedValues, i);
        }


        /// <summary>
        /// Performs the regression at a single time.
        /// </summary>
        /// <param name="pathwiseCfValues">The pathwise cashflow values.</param>
        /// <param name="pathwiseIndependent">The pathwise independent variables.</param>
        /// <param name="regressedValues">The regressed values.</param>
        /// <param name="col">The column on which regression should be performed.</param>
        private void PerformRegression(double[,] pathwiseCfValues, double[,,] pathwiseIndependent, 
            double[,] regressedValues, int col)
        {
            // We will use Ordinary Least Squares to create a
            // linear regression model with an intercept term
            var ols = new OrdinaryLeastSquares()
            { UseIntercept = true, IsRobust = true };

            // Create the inputs and outputs
            double[][] inputs = new double[pathwiseIndependent.GetLength(0)][];
            for (int row = 0; row < pathwiseIndependent.GetLength(0); row++)
            {
                double[] rowValues = new double[1 + 3 * pathwiseIndependent.GetLength(2)];
                rowValues[0] = 1;
                for (int i = 0; i < pathwiseIndependent.GetLength(2); i++)
                {
                    double x = pathwiseIndependent[row, col, i];
                    rowValues[1 + i * 3] = x;
                    rowValues[1 + i * 3 + 1] = x * x;
                    rowValues[1 + i * 3 + 2] = x * x * x;
                }
                inputs[row] = rowValues;
            }
            double[] outputs = pathwiseCfValues.GetColumn(col);

            // Use Ordinary Least Squares to estimate a regression model
            MultipleLinearRegression regression = ols.Learn(inputs, outputs);
            double[] fitted = regression.Transform(inputs);
            regressedValues.SetColumn(col, fitted);
        }


        /// <summary>
        /// Performs a chunk of simulations
        /// </summary>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <param name="pathwiseCfValues">The pathwise cf values.</param>
        /// <param name="regressedValues">The regressed values.</param>
        /// <param name="pathwiseIndependent">The pathwise independent.</param>
        /// <param name="pathStart">The path start.</param>
        /// <param name="pathEnd">The path end.</param>
        private void PerformSimulationChunk(List<Date> fwdValueDates, double[,] pathwiseCfValues,
            double[,] regressedValues, double[,,] pathwiseIndependent, int pathStart, int pathEnd)
        {
            // clone the simulators and portfolio
            List<Product> localPortfolio = portfolio.Clone();
            NumeraireSimulator localNumeraire = null;
            List<Simulator> localSimulators = null;
            CopySimulators(out localNumeraire, out localSimulators);
                        
            for (int pathCounter = pathStart; pathCounter < pathEnd; pathCounter++)
            {
                double numeraireAtValue = localNumeraire.Numeraire(valueDate);
                // Run the simulation 
                foreach (Simulator simulator in localSimulators)
                {
                    simulator.RunSimulation(pathCounter);
                }
                // get the underlying factors
                for (int fwdDateCounter = 0; fwdDateCounter < fwdValueDates.Count; fwdDateCounter++)
                {
                    int independentCounter = 0;
                    foreach (Simulator simulator in localSimulators)
                    {
                        double[] underlyingFactors = simulator.GetUnderlyingFactors(fwdValueDates[fwdDateCounter]);
                        for (int thisIndependentCounter = 0; thisIndependentCounter < underlyingFactors.Length; thisIndependentCounter++)
                        {
                            pathwiseIndependent[pathCounter, fwdDateCounter, independentCounter] = underlyingFactors[thisIndependentCounter];
                            independentCounter++;
                        }
                    }
                }
                // use the simulators that now contain a simulation to provide market observables to the 
                // products.
                foreach (Product product in localPortfolio)
                {
                    product.Reset();
                    foreach (MarketObservable index in product.GetRequiredIndices())
                    {
                        Simulator simulator = localSimulators[indexSources[index]];
                        List<Date> requiredDates = product.GetRequiredIndexDates(index);
                        double[] indices = simulator.GetIndices(index, requiredDates);
                        product.SetIndexValues(index, indices);
                    }
                    List<Cashflow> timesAndCFS = product.GetCFs();
                    foreach (Cashflow cf in timesAndCFS)
                    {
                        double cfValue;
                        if (cf.currency.Equals(valueCurrency))
                        {
                            cfValue = cf.amount * numeraireAtValue / localNumeraire.Numeraire(cf.date);
                        }
                        else
                        {
                            MarketObservable currencyPair = new CurrencyPair(cf.currency, localNumeraire.GetNumeraireCurrency());                            
                            Simulator simulator = localSimulators[indexSources[currencyPair]];
                            double fxRate = simulator.GetIndices(currencyPair, new List<Date> { cf.date })[0];
                            cfValue = fxRate * cf.amount * numeraireAtValue / localNumeraire.Numeraire(cf.date);
                        }
                        for (int fwdDateCounter = 0; fwdDateCounter < fwdValueDates.Count; fwdDateCounter++)
                        {
                            if (cf.date > fwdValueDates[fwdDateCounter])
                            {
                                pathwiseCfValues[pathCounter, fwdDateCounter] += cfValue;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Copies the simulators into local variables so that they can be safely used on a thread 
        /// without changing the originals.
        /// </summary>
        /// <param name="localNumeraire">The local numeraire.  The reference to this object is changed in the call.</param>
        /// <param name="localSimulators">The local simulators.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void CopySimulators(out NumeraireSimulator localNumeraire, out List<Simulator> localSimulators)
        {
            localSimulators = new List<Simulator>();
            foreach (Simulator simulator in simulators)
            {
                localSimulators.Add(simulator.Clone());
            }
            localNumeraire = (NumeraireSimulator)localSimulators[0];
        }


        /// <summary>
        /// Find the value of a portfolio of products
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="valueDate">The value date.</param>
        /// <returns></returns>
        public double Value(List<Product> portfolio, Date valueDate)
        {
            this.portfolio = portfolio;
            this.valueDate = valueDate;
            AssociateFactorsWithSimulators();
            InitializeSimulators(new List<Date>());
            
            // Run the simulation
            // TODO: Rather store value for each product separately
            double[] pathwiseValues = new double[N];
            double totalValue = 0;            


            for (int i = 0; i < N; i++) // This N loop can be made parallel, need to check the size and cost 
                                        // of copying all the simualators and products 
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
                        Simulator simulator = simulators[indexSources[index]];
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
                            Simulator simulator = simulators[indexSources[currencyPair]];
                            double fxRate = simulator.GetIndices(currencyPair, new List<Date> { cf.date })[0];
                            pathwiseValues[i] += fxRate * cf.amount * numeraireAtValue / numeraire.Numeraire(cf.date);
                        }
                    }
                }
                totalValue += pathwiseValues[i];
            }
            return totalValue / N;

        }

        /// <summary>
        /// Initializes the simulators by telling them at which dates they will need to provide which market indices.
        /// </summary>
        /// <param name="extraDates">simualators .</param>
        private void InitializeSimulators(List<Date> extraDates)
        {
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
                    simulators[indexSources[index]].SetRequiredDates(index, requiredTimes);
                }
                // Tell the nummeraire simulator at what times it will be required.
                // Tell the FX simulators at what times they will be required.
                foreach (Currency ccy in product.GetCashflowCurrencies())
                {
                    List<Date> requiredDates = product.GetCashflowDates(ccy);
                    numeraire.SetNumeraireDates(requiredDates);
                    if (ccy != numeraire.GetNumeraireCurrency())
                    {
                        MarketObservable index = new CurrencyPair(ccy, numeraire.GetNumeraireCurrency());
                        simulators[indexSources[index]].SetRequiredDates(index, requiredDates);
                    }
                }
            }

            // Prepare all the simulators
            foreach (Simulator simulator in simulators)
            { simulator.Prepare(); }
        }

        /// <summary>
        /// Creates a lookup so that the coordinator knows which simulator provides each required index.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Cannot use 'ANY' as the valuation currency when the portfolio includes cashflows in multiple currencies.
        /// </exception>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Required index: " + index.ToString() + " is not provided by any of the simulators.
        /// or
        /// Required currency pair: " + index.ToString() + " is not provided by any of the simulators
        /// </exception>
        private void AssociateFactorsWithSimulators()
        {
            // Find which simulator will provide each of the potentially required MarketObservables.
            indexSources = new Dictionary<MarketObservable, int>();
            HashSet<Currency> requiredCurrencySet = new HashSet<Currency>();
            foreach (Product product in portfolio)
            {
                // Associate the index simulators
                foreach (MarketObservable index in product.GetRequiredIndices())
                {
                    if (!indexSources.ContainsKey(index))
                    {
                        bool found = false;
                        for (int simulatorCounter=0; simulatorCounter< simulators.Count; simulatorCounter++)
                        {
                            if (simulators[simulatorCounter].ProvidesIndex(index))
                            {
                                if (!found)
                                {
                                    indexSources[index] = simulatorCounter;
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
                        for (int simulatorCounter = 0; simulatorCounter < simulators.Count; simulatorCounter++)
                        {
                            if (simulators[simulatorCounter].ProvidesIndex(index))
                            {
                                indexSources[index] = simulatorCounter;
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
        }
    }
}
