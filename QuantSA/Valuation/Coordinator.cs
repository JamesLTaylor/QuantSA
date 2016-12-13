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
        // Settings
        private bool useThreads = true;
        int maxThreads = 4;
        private int N;

        // Simulators and associated data
        private NumeraireSimulator numeraire;
        private List<Simulator> simulators;
        Dictionary<MarketObservable, int> indexSources;
        Date valueDate;
        Currency valueCurrency;
        List<Date> allDates;

        // Portfolio information
        List<Product> allTrades;
        List<int> originalTrades;
        /// <summary>
        /// Trades in <see cref="allTrades"/> that are options with the indices of what they exercise into.
        /// </summary>
        Dictionary<int, List<int>> postExerciseTrades;
        
        // Simulated data
        SimulatedCashflows simulatedCFs;
        SimulatedRegressors simulatedRegs;
        double[,] regressedValues;
        


        /// <summary>
        /// The main constructor for the Coordinator.
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
        private void PerformSimulationChunk(List<Product> portfolio, List<Date> fwdValueDates,
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
                for (int productCounter = 0; productCounter < localPortfolio.Count; productCounter++)
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
        /// Prepares the portfolios by splitting produicts with early exercise into 
        /// the products they become after exercise and the product that produces their
        /// cashflows until exercise.
        /// </summary>
        /// <param name="portfolioIn">The user supplied portfolio.</param>
        /// <param name="fwdValueDates">The extra forward value dates.</param>
        private void PreparePortfolios(Product[] portfolioIn, Date[] fwdValueDates)
        {            
            allDates = fwdValueDates.Select(date => new Date(date)).ToList();
            allDates.Add(valueDate);
            allTrades = new List<Product>();
            originalTrades = new List<int>();
            postExerciseTrades = new Dictionary<int, List<int>>();

            // Add the orginal trades
            int counter = 0;
            foreach (Product product in portfolioIn)
            {
                allTrades.Add(product.Clone());
                originalTrades.Add(counter);                
                ProductWithEarlyExercise option = product as ProductWithEarlyExercise;
                if (option != null)
                {
                    List<int> subList = new List<int>();
                    postExerciseTrades.Add(counter, subList);
                    counter++;
                    allDates.AddRange(option.GetExerciseDates());
                    List<Product> postExProducts = option.GetPostExProducts();
                    foreach (Product postExProduct in postExProducts)
                    {
                        allTrades.Add(postExProduct);
                        subList.Add(counter);
                        counter++;
                    }
                }
                else counter++;
            }
            allDates = allDates.Distinct().ToList();
            allDates.Sort();
        }

        /// <summary>
        /// Replaces the no exercise cashflows for the product at position <paramref name="key"/> with the cashflows based on 
        /// an estimated optimal exercise policy.
        /// </summary>
        /// <param name="key">The postion in <see cref="allTrades"/> of the product to be updated.</param>
        private void ApplyEarlyExercise(int key)
        {
            // Get pathwise regressed values of post exercise products.
            ProductWithEarlyExercise option = allTrades[key] as ProductWithEarlyExercise;
            List<Date> exDates = option.GetExerciseDates();
            double[,] postExRegressedValues = new double[N, exDates.Count];
            for (int i = 0; i < exDates.Count; i++)
            {
                // perform regression on each exercise date                
                int postExerciseProductInd = postExerciseTrades[key][option.GetPostExProductAtDate(exDates[i])];
                double[] fitted = PerformRegression(exDates[i], simulatedCFs, simulatedRegs, new List<int> { postExerciseProductInd });
                postExRegressedValues.SetColumn(i, fitted);
            }

            // Iterate backwards
            // initially the stoppping time on all paths is infinity (actually the year 3000)
            Date[] optimalStop = new Date[N];
            for (int i = 0; i < N; i++) optimalStop[i] = new Date(3000, 1, 1);

            for (int exDateCount = exDates.Count - 1; exDateCount >= 0; exDateCount--)
            {
                Date exDate = exDates[exDateCount];
                int exProductInd = postExerciseTrades[key][option.GetPostExProductAtDate(exDate)];                
                // Optimal flows are underlying product up to the stopping time, then the post exercise product flows afterwards
                double[] pvOptimalCFs = Vector.Zeros(N);
                for (int pathCount = 0; pathCount < N; pathCount++)
                {
                    foreach (Cashflow cf in simulatedCFs.GetCFs(exProductInd, pathCount))
                    {
                        if (cf.date > optimalStop[pathCount])
                            pvOptimalCFs[pathCount] += cf.amount;
                    }
                    foreach (Cashflow cf in simulatedCFs.GetCFs(key, pathCount))
                    {
                        if (cf.date > valueDate && cf.date <= optimalStop[pathCount])
                            pvOptimalCFs[pathCount] += cf.amount;
                    }
                }

                // update optimal stopping times
                double[] optimalCV = simulatedRegs.FitCFs(exDate, pvOptimalCFs);

                for (int pathCount = 0; pathCount < N; pathCount++)
                {
                    if (option.IsLongOptionality(exDate) && optimalCV[pathCount] < postExRegressedValues[pathCount, exDateCount])
                        optimalStop[pathCount] = exDate;
                    else if (!option.IsLongOptionality(exDate) && optimalCV[pathCount] > postExRegressedValues[pathCount, exDateCount])
                        optimalStop[pathCount] = exDate;
                }
            }
            // All stopping times have been found so now we can update the cashflows.  
            // The cashflows are continuation flows up to the exercise date then cashflows from the 
            // exercise product after that.
            List<Cashflow>[] newCFs = new List<Cashflow>[N];
            for (int pathCount = 0; pathCount < N; pathCount++)
            {                
                newCFs[pathCount] = new List<Cashflow>();
                int exProductInd = postExerciseTrades[key][option.GetPostExProductAtDate(optimalStop[pathCount])];
                foreach (Cashflow cf in simulatedCFs.GetCFs(exProductInd, pathCount))
                {
                    if (cf.date > optimalStop[pathCount])
                        newCFs[pathCount].Add(cf);
                }
                foreach (Cashflow cf in simulatedCFs.GetCFs(0, pathCount))
                {
                    if (cf.date > valueDate && cf.date <= optimalStop[pathCount])
                        newCFs[pathCount].Add(cf);
                }
            }
            simulatedCFs.Update(key, newCFs);            
        }


        /// <summary>
        /// Applies the early exercise conditions to all the products that require it.  Calls 
        /// <see cref="ApplyEarlyExercise(int)"/> on a separate thread for each early exercise 
        /// product.
        /// </summary>
        private void ApplyEarlyExercise()
        {
            if (useThreads)
            {
                Thread[] earlyExThreads = new Thread[postExerciseTrades.Keys.Count];
                int i = 0;
                foreach (int key in postExerciseTrades.Keys)
                {
                    earlyExThreads[i] = new Thread(
                        new ThreadStart(
                            () => ApplyEarlyExercise(key))
                        );
                    earlyExThreads[i].Start();
                    i++;
                }
                foreach (Thread thread in earlyExThreads) thread.Join();
            }
            else
            {
                foreach (int key in postExerciseTrades.Keys)
                {
                    ApplyEarlyExercise(key);
                }
            }
        }

        /// <summary>
        /// Performs the simulation of all cashflows.  These may need to be adjusted for early exercise.
        /// </summary>
        private void PerformSimulation()
        {
            // Run the simulation in chunks on several threads.
            if (useThreads && N >= 1000)
            {                
                Thread[] simThreads = new Thread[maxThreads];
                int simChunkSize = (int)Math.Ceiling(N / (double)maxThreads);
                for (int i = 0; i < maxThreads; i++)
                {
                    int start = i * simChunkSize;
                    int end = Math.Min(start + simChunkSize, N);
                    simThreads[i] = new Thread(
                        new ThreadStart(
                            () => PerformSimulationChunk(allTrades, allDates, simulatedCFs, simulatedRegs, start, end))
                        );
                    simThreads[i].Start();
                }
                foreach (Thread thread in simThreads) thread.Join();
            }
            else
            {
                PerformSimulationChunk(allTrades, allDates, simulatedCFs, simulatedRegs, 0, N);
            }
        }


        /// <summary>
        /// Update the conditional forward PVs over a range.
        /// </summary>
        /// <param name="fwdValueDates">The full set of forward value dates.</param>
        /// <param name="startIndex">The start index of the range.  Forward values after this date will be updated.</param>
        /// <param name="endIndex">The end index of the range.  forward values stricly before this date will be updated.</param>
        /// <param name="regressedValues">The full set of estimated forward values.</param>
        private void ApplyForwardValueRegressionsChunk(List<Date> fwdValueDates, int startIndex, int endIndex, double[,] regressedValues)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                // perform regression on each forward date
                double[] fitted = PerformRegression(fwdValueDates[i], simulatedCFs, simulatedRegs, originalTrades);
                regressedValues.SetColumn(i, fitted);
            }
        }

        /// <summary>
        /// Update the all the conditional forward PVs.  Will break them into ranges and 
        /// call <see cref="ApplyForwardValueRegressionsChunk(List{Date}, int, int, double[,])"/>
        /// </summary>
        /// <param name="fwdValueDates">The dates at which forward values are required.</param>
        private double[,] ApplyForwardValueRegressions(List<Date> fwdValueDates)
        {
            double[,] regressedValues = new double[N, fwdValueDates.Count()];

            if (fwdValueDates.Count>= 20 && useThreads)
            {
                Thread[] regressThreads = new Thread[maxThreads];
                int regressChunkSize = (int)Math.Ceiling(fwdValueDates.Count() / (double)maxThreads);
                for (int i = 0; i < maxThreads; i++)
                {
                    int start = i * regressChunkSize;
                    int end = Math.Min(start + regressChunkSize, fwdValueDates.Count());
                    regressThreads[i] = new Thread(
                        new ThreadStart(
                            () => ApplyForwardValueRegressionsChunk(fwdValueDates, start, end, regressedValues))
                        );
                    regressThreads[i].Start();
                }
                foreach (Thread thread in regressThreads) thread.Join();
            }
            else
            {
                ApplyForwardValueRegressionsChunk(fwdValueDates, 0, fwdValueDates.Count(), regressedValues);
            }
            return regressedValues;
        }

        /// <summary>
        /// Performs the regression for a single date
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="simulatedCFs">The simulated c fs.</param>
        /// <param name="simulatedRegs">The simulated regs.</param>
        /// <param name="subPortfolio">The sub portfolio.</param>
        /// <returns></returns>
        private double[] PerformRegression(Date date, SimulatedCashflows simulatedCFs, SimulatedRegressors simulatedRegs,
            List<int> subPortfolio)
        {
            double[] cfs = simulatedCFs.GetPathwisePV(date, subPortfolio);
            double[] fitted = simulatedRegs.FitCFs(date, cfs);
            return fitted;
        }


        /// <summary>
        /// Initializes the Coordinator, runs the suimulation, applies early exercise rules, 
        /// derives forward value paths.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        private void CalculateAll(Product[] portfolioIn, Date valueDate, Date[] fwdValueDates)
        {
            this.valueDate = valueDate;
            PreparePortfolios(portfolioIn, fwdValueDates);
            AssociateFactorsWithSimulators(allTrades);
            InitializeSimulators(allTrades, new List<Date>());
            simulatedCFs = new SimulatedCashflows(allTrades.Count, N); // initialized outside to allow multiple threads.
            simulatedRegs = new SimulatedRegressors(allDates, N, simulators);
            PerformSimulation();
            ApplyEarlyExercise();
            if (fwdValueDates.Length>0) // Only regress is forward values are required.
                regressedValues = ApplyForwardValueRegressions(fwdValueDates.ToList());
        }
        

        /// <summary>
        /// Gets all the data that might be of interest after a simulation.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        public ResultStore GetValuePaths(Product[] portfolioIn, Date valueDate, Date[] fwdValueDates)
        {
            CalculateAll(portfolioIn, valueDate, fwdValueDates);
            ResultStore results = new ResultStore();
            return results;

        }


        /// <summary>
        /// Calcualte the expected positive exposure on a portfolio.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        public double[] EPE(Product[] portfolioIn, Date valueDate, Date[] fwdValueDates)
        {
            CalculateAll(portfolioIn, valueDate, fwdValueDates);

            double[] epe = Vector.Zeros(fwdValueDates.Length);
            List<Date> fwdValueDatesList = fwdValueDates.ToList();

            //Debug.WriteToFile(@"c:\dev\temp\regressedValues.csv", regressedValues);            
            for (int row = 0; row < regressedValues.GetLength(0); row++)
            {
                for (int col = 0; col < regressedValues.GetLength(1); col++)
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
        /// Values the specified portfolio.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <returns></returns>
        public double Value(Product[] portfolioIn, Date valueDate)
        {
            CalculateAll(portfolioIn, valueDate, new Date[0]);
            double[] pathwisePVs = simulatedCFs.GetPathwisePV(valueDate, originalTrades);
            return pathwisePVs.Average();
        }


        /// <summary>
        /// Initializes the simulators by telling them at which dates they will need to provide which market indices.
        /// </summary>
        /// <param name="extraDates">Extra dates over and above the contract dates where the simulators will need to 
        /// provide their indices.</param>
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
