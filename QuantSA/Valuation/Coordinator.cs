using Accord.Math;
using Accord.Statistics.Models.Regression.Linear;
using QuantSA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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


        public double ValueWithEarlyEx(Product[] portfolio, Date valueDate)
        {
            this.valueDate = valueDate;
            BermudanSwaption earlyExProduct = (BermudanSwaption)portfolio[0];
            List<Product> postExProducts = earlyExProduct.GetPostExProducts();
            List<Product> extendedPortolfio = portfolio.ToList();
            extendedPortolfio.AddRange(postExProducts);
            AssociateFactorsWithSimulators(extendedPortolfio);
            List<Date> exDates = earlyExProduct.GetExerciseDates();            
            exDates.Sort();
            InitializeSimulators(extendedPortolfio, exDates);

            // Produce data: product -> list effective cfs.  amount is converted to value currency and deflated to value date.
            SimulatedCashflows simulatedCFs = new SimulatedCashflows(extendedPortolfio, N);            
            SimulatedRegressors simulatedRegs = new SimulatedRegressors(exDates, N, simulators);            
            PerformSimulationChunk2(extendedPortolfio, exDates, simulatedCFs, simulatedRegs, 0, N);

            // Get pathwise regressed values of post exercise products.
            double[,] postExRegressedValues = new double[N, exDates.Count];
            for (int i = 0; i<exDates.Count; i++)
            {
                // perform regression on each exercise date
                int exerciseProductIndex = earlyExProduct.GetPostExProductAtDate(exDates[i]);
                List<Product> exerciseProductSubPortfolio = new List<Product> { extendedPortolfio[1 + exerciseProductIndex] };
                double[] fitted = PerformRegression2(exDates[i], simulatedCFs, simulatedRegs, exerciseProductSubPortfolio);
                postExRegressedValues.SetColumn(i, fitted);
            }

            // Iterate backwards
            // initially the stoppping time on all paths is infinity
            Date[] optimalStop = new Date[N];
            for (int i = 0; i < N; i++) optimalStop[i] = new Date(3000, 1, 1);
            
            for (int exDateCount = exDates.Count - 1; exDateCount >= 0; exDateCount--)
            {
                Date exDate = exDates[exDateCount];
                int exProductInd = 1+earlyExProduct.GetPostExProductAtDate(exDate);
                // Optimal flows are underlying product up to the stopping time, then the post exercise product flows afterwards
                double[] pvOptimalCFs = Vector.Zeros(N);
                for (int pathCount = 0; pathCount < N; pathCount++)
                {
                    foreach (Cashflow cf in simulatedCFs.GetCFs(exProductInd, pathCount))
                    {
                        if (cf.date > optimalStop[pathCount])
                            pvOptimalCFs[pathCount] += cf.amount;
                    }
                    foreach (Cashflow cf in simulatedCFs.GetCFs(0, pathCount))
                    {
                        if (cf.date > valueDate && cf.date <= optimalStop[pathCount])
                            pvOptimalCFs[pathCount] += cf.amount;
                    }
                }

                // update optimal stopping times
                var ols = new OrdinaryLeastSquares()
                { UseIntercept = true, IsRobust = true };
                double[][] inputs = simulatedRegs.GetPolynomialValsRegular(exDate);                                
                MultipleLinearRegression regression = ols.Learn(inputs, pvOptimalCFs);
                double[] optimalCV = regression.Transform(inputs);

                for (int pathCount = 0; pathCount < N; pathCount++)
                {
                    if (optimalCV[pathCount] < postExRegressedValues[pathCount, exDateCount])
                        optimalStop[pathCount] = exDate;
                }
            }
            return 0.0;
        }

        private double[] PerformRegression2(Date date, SimulatedCashflows simulatedCFs, SimulatedRegressors simulatedRegs, 
            List<Product> subPortfolio)
        {
            // We will use Ordinary Least Squares to create a
            // linear regression model with an intercept term
            var ols = new OrdinaryLeastSquares()
            { UseIntercept = true, IsRobust = true };

            // Create the inputs and outputs
            double[][] inputs = simulatedRegs.GetPolynomialValsRegular(date);
            double[] outputs = simulatedCFs.GetPathwisePV(date, subPortfolio);

            // Use Ordinary Least Squares to estimate a regression model
            MultipleLinearRegression regression = ols.Learn(inputs, outputs);
            double[] fitted = regression.Transform(inputs);
            return fitted;            
        }


        /// <summary>
        /// Gets the regressed values.
        /// </summary>
        /// <param name="pvCFs">The sum of the present value of all cashflows on each path in a single currency.</param>
        /// <param name="independentValues">The independent values to be used in the regression.</param>
        /// <returns></returns>
        private double[] GetRegressedValues(double[] pvCFs, double[,] independentValues)
        {
            return new double[pvCFs.Length];
        }



        /// <summary>
        /// Epes the specified portfolio.
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        /// <remarks>
        /// The simulation phase should possibly produce some set of name,value pairs like:
        /// productID:simNumber:date,cfValueInNumeraireCcy
        /// So that fully flexible distributed downstream calculations might be possible.
        /// </remarks>
        public double[] EPE(Product[] portfolio, Date valueDate, Date[] fwdValueDates)
        {
            double[] epe = Vector.Zeros(fwdValueDates.Length);
            List<Date> fwdValueDatesList = fwdValueDates.ToList();
            double[,] regressedValues = GetRegressedFwdValues(portfolio.ToList(), valueDate, fwdValueDatesList);
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
        /// <remarks>This was first written using tasks rather than threads but the tasks were not getting their
        /// own thread, even when marked as <see cref="TaskCreationOptions.LongRunning"/></remarks>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        private double[,] GetRegressedFwdValues(List<Product> portfolio, Date valueDate, List<Date> fwdValueDates)
        {
            this.valueDate = valueDate;
            AssociateFactorsWithSimulators(portfolio);
            InitializeSimulators(portfolio, fwdValueDates);

            // run one simulation to see how many indepedent variables each simulator provides
            int independentCount = 0;
            foreach (Simulator simulator in simulators)
            {
                simulator.RunSimulation(0);
                independentCount += simulator.GetUnderlyingFactors(fwdValueDates[0]).Length;
            }

            int nTasks = 4;
            double[,] pathwiseCfValues = new double[N, fwdValueDates.Count];
            double[,] regressedValues = new double[N, fwdValueDates.Count];
            double[,,] pathwiseIndependent = new double[N, fwdValueDates.Count, independentCount];

            // Run the simulation in chunks on several threads.
            Thread[] simThreads = new Thread[nTasks];
            int simChunkSize = (int)Math.Ceiling(N / (double)nTasks);
            for (int i = 0; i < nTasks; i++)
            {
                int start = i * simChunkSize;
                int end = Math.Min(start + simChunkSize, N);
                simThreads[i] = new Thread(
                    new ThreadStart(
                        () => PerformSimulationChunk(portfolio, fwdValueDates, pathwiseCfValues, pathwiseIndependent, start, end))
                    );
                simThreads[i].Start();
            }
            foreach (Thread thread in simThreads) thread.Join();

            Thread[] regressThreads = new Thread[nTasks];
            int regressChunkSize = (int)Math.Ceiling(fwdValueDates.Count() / (double)nTasks);
            for (int i = 0; i < nTasks; i++)
            {
                int start = i * regressChunkSize;
                int end = Math.Min(start + regressChunkSize, fwdValueDates.Count());
                regressThreads[i] = new Thread(
                    new ThreadStart(
                        () => PerformRegressionChunk(pathwiseCfValues, pathwiseIndependent, regressedValues, start, end))
                        );
                regressThreads[i].Start();

            }
            foreach (Thread thread in regressThreads) thread.Join();
            return regressedValues;
        }

        /// <summary>
        /// Performs the regression at a range of times.
        /// </summary>
        /// <param name="pathwiseIndependent">indexed by pathNum, fwdDateNum, factorNum</param>
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
        /// Performs the simulation chunk that updates 
        ///     the cashflows per product and per path.
        ///     the 
        /// </summary>
        /// <param name="portfolio"></param>
        /// <param name="fwdValueDates">The dates at which the underlying factors will be saved for later regression.</param>
        /// <param name="simulatedCFs">The cashflows converted to the value currency and deflated witht he numeaire. Updated 
        /// by this function.</param>
        /// <param name="simulatedRegs">The valuesor the regressors by path and date.</param>
        /// <param name="pathStart">The path start.</param>
        /// <param name="pathEnd">The path end.</param>
        private void PerformSimulationChunk2(List<Product> portfolio, List<Date> fwdValueDates,
            SimulatedCashflows simulatedCFs, SimulatedRegressors simulatedRegs, int pathStart, int pathEnd)
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
                            simulatedRegs.Add(pathCounter, fwdDateCounter, independentCounter, underlyingFactors[thisIndependentCounter]);
                            independentCounter++;
                        }
                    }
                }
                // use the simulators that now contain a simulation to provide market observables to the 
                // products.
                for (int productCounter = 0; productCounter< localPortfolio.Count; productCounter++)
                {
                    Product product = localPortfolio[productCounter];
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
                        if (cf.currency.Equals(valueCurrency))
                        {
                            double cfValue = cf.amount * numeraireAtValue / localNumeraire.Numeraire(cf.date);
                            simulatedCFs.Add(productCounter, pathCounter, new Cashflow(cf.date, cfValue, cf.currency));
                        }
                        else
                        {
                            MarketObservable currencyPair = new CurrencyPair(cf.currency, localNumeraire.GetNumeraireCurrency());
                            Simulator simulator = localSimulators[indexSources[currencyPair]];
                            double fxRate = simulator.GetIndices(currencyPair, new List<Date> { cf.date })[0];
                            double cfValue = fxRate * cf.amount * numeraireAtValue / localNumeraire.Numeraire(cf.date);
                            simulatedCFs.Add(productCounter, pathCounter, new Cashflow(cf.date, cfValue, cf.currency));
                        }
                    }                    
                }
            }
        }


        /// <summary>
        /// Performs a chunk of simulations
        /// </summary>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <param name="pathwiseCfValues">The pathwise cf values.</param>
        /// <param name="pathwiseIndependent">The pathwise independent.</param>
        /// <param name="pathStart">The path start.</param>
        /// <param name="pathEnd">The path end.</param>
        private void PerformSimulationChunk(List<Product> portfolio, List<Date> fwdValueDates, double[,] pathwiseCfValues,
            double[,,] pathwiseIndependent, int pathStart, int pathEnd)
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
        public double Value(Product[] portfolioIn, Date valueDate)
        {
            List<Product> portfolio = portfolioIn.ToList();            
            this.valueDate = valueDate;
            AssociateFactorsWithSimulators(portfolio);
            InitializeSimulators(portfolio, new List<Date>());
            
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
        private void InitializeSimulators(List<Product> portfolio, List<Date> extraDates)
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
        private void AssociateFactorsWithSimulators(List<Product> portfolio)
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
