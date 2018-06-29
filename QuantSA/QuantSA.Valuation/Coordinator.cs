using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using QuantSA.Core.Primitives;
using QuantSA.Core.Serialization;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

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
    /// 7: The simulator keeps track of the portfolio's cashflows across each realization of a state of the world.
    /// </remarks>
    public class Coordinator
    {
        private readonly List<Simulator> _availableSimulators;
        private readonly int _numberOfPaths;

        // Simulators and associated data
        private readonly NumeraireSimulator _numeraireSimulator;
        private List<Date> _allDates;

        // Portfolio information
        private List<IProduct> _allTrades;
        private int _maxThreads = 4;
        private List<int> _originalTrades;

        /// <summary>
        /// Trades in <see cref="_allTrades"/> that are options with the indices of what they exercise into.
        /// </summary>
        private Dictionary<int, List<int>> _postExerciseTrades;

        private double[,] _regressedValues;

        // Simulated data
        private SimulatedCashflows _simulatedCFs;

        private SimulatedRegressors _simulatedRegs;

        // Settings
        private bool _useThreads = true;

        private Date _valueDate;


        /// <summary>
        /// The main constructor for the Coordinator.
        /// </summary>
        /// <param name="numeraireSimulator">A special simulator that also includes the numeraire. There is only one of these.</param>        
        /// <param name="availableSimulators">Optionally any extra simulators independent of the first.  Can be an empty list.</param>
        /// <param name="numberOfPaths"></param>
        public Coordinator(NumeraireSimulator numeraireSimulator, List<Simulator> availableSimulators,
            int numberOfPaths)
        {
            _numeraireSimulator = numeraireSimulator;
            _availableSimulators = availableSimulators;
            _numberOfPaths = numberOfPaths;
        }

        /// <summary>
        /// Specifies if threads should be used.  If it set to <c>true</c> then the default maximum number of threads will be used.
        /// </summary>
        /// <seealso cref="SetThreadedness(bool, int)"/>
        /// <param name="useThreads">if set to <c>true</c> then the coordinator will perform calculation on multiple threads.</param>
        public void SetThreadedness(bool useThreads)
        {
            _useThreads = useThreads;
        }

        /// <summary>
        /// Specifies if threads should be used and if so what the maximum number of threads should be.
        /// </summary>
        /// <seealso cref="SetThreadedness(bool)"/>
        /// <param name="useThreads">if set to <c>true</c> then the coordinator will perform calculation on multiple threads.</param>
        /// <param name="maxThreads">The maximum number of threads to use.</param>
        public void SetThreadedness(bool useThreads, int maxThreads)
        {
            _useThreads = useThreads;
            _maxThreads = maxThreads;
        }


        /// <summary>
        /// Performs the simulation chunk that updates 
        ///     the cashflows per product and per path.
        ///     the 
        /// </summary>
        /// <param name="portfolio"></param>
        /// <param name="allDates">The dates at which the underlying factors will be saved for later regression.</param>
        /// <param name="simulatedCFs">The cashflows converted to the value currency and deflated with the numeraire. Updated 
        /// by this function.</param>
        /// <param name="simulatedRegs">The values of the regressors by path and date.</param>
        /// <param name="pathStart">The path start.</param>
        /// <param name="pathEnd">The path end.</param>
        private void PerformSimulationChunk(List<IProduct> portfolio, List<Date> allDates,
            SimulatedCashflows simulatedCFs, SimulatedRegressors simulatedRegs, int pathStart, int pathEnd)
        {
            // clone the simulators and portfolio if this is running multi threaded
            var localPortfolio = portfolio.Clone();

            var (localNumeraire, localSimulators) = GetCopyOfSimulators();
            var mappedSimulators = AssociateFactorsWithSimulators(localPortfolio, localNumeraire, localSimulators);
            PrepareSimulators(_valueDate, localPortfolio, allDates, localNumeraire, mappedSimulators,
                localSimulators);

            foreach (var product in localPortfolio)
                product.SetValueDate(_valueDate);

            for (var pathCounter = pathStart; pathCounter < pathEnd; pathCounter++)
            {
                var numeraireAtValue = localNumeraire.Numeraire(_valueDate);
                // Run the simulation 
                foreach (var simulator in localSimulators) simulator.RunSimulation(pathCounter);

                // get the underlying factors
                for (var fwdDateCounter = 0; fwdDateCounter < allDates.Count; fwdDateCounter++)
                {
                    var independentCounter = 0;
                    foreach (var simulator in localSimulators)
                    {
                        var underlyingFactors = simulator.GetUnderlyingFactors(allDates[fwdDateCounter]);
                        foreach (var factor in underlyingFactors)
                        {
                            simulatedRegs.Add(pathCounter, fwdDateCounter, independentCounter, factor);
                            independentCounter++;
                        }
                    }
                }

                // use the simulators that now contain a simulation to provide market observables to the 
                // products.
                for (var productCounter = 0; productCounter < localPortfolio.Count; productCounter++)
                {
                    var product = localPortfolio[productCounter];
                    product.Reset();
                    foreach (var index in product.GetRequiredIndices())
                    {
                        var simulator = mappedSimulators[index];
                        var requiredDates = product.GetRequiredIndexDates(index);
                        var indices = simulator.GetIndices(index, requiredDates);
                        product.SetIndexValues(index, indices);
                    }

                    var timesAndCfs = product.GetCFs();
                    foreach (var cf in timesAndCfs)
                        if (cf.Currency.Equals(_numeraireSimulator.GetNumeraireCurrency()))
                        {
                            var cfValue = cf.Amount * numeraireAtValue / localNumeraire.Numeraire(cf.Date);
                            simulatedCFs.Add(productCounter, pathCounter, new Cashflow(cf.Date, cfValue, cf.Currency));
                        }
                        else
                        {
                            MarketObservable currencyPair =
                                new CurrencyPair(cf.Currency, localNumeraire.GetNumeraireCurrency());
                            var simulator = mappedSimulators[currencyPair];
                            var fxRate = simulator.GetIndices(currencyPair, new List<Date> {cf.Date})[0];
                            var cfValue = fxRate * cf.Amount * numeraireAtValue / localNumeraire.Numeraire(cf.Date);
                            simulatedCFs.Add(productCounter, pathCounter, new Cashflow(cf.Date, cfValue, cf.Currency));
                        }
                }
            }
        }

        /// <summary>
        /// Copies the simulators into local variables so that they can be safely used on a thread 
        /// without changing the originals.  The first element is the numeraire simulator.
        /// </summary>
        private (NumeraireSimulator, List<Simulator>) GetCopyOfSimulators()
        {
            var numeraireSim = (NumeraireSimulator) Cloner.Clone(_numeraireSimulator);
            var allSims = new List<Simulator> {numeraireSim};
            allSims.AddRange(_availableSimulators.Select(s => (Simulator)Cloner.Clone(s)));
            return (numeraireSim, allSims);
        }

        /// <summary>
        /// Prepares the portfolios by splitting products with early exercise into 
        /// the products they become after exercise and the product that produces their
        /// cashflows until exercise.
        /// </summary>
        /// <param name="portfolioIn">The user supplied portfolio.</param>
        /// <param name="fwdValueDates">The extra forward value dates.</param>
        private void PreparePortfolios(IProduct[] portfolioIn, Date[] fwdValueDates)
        {
            _allDates = fwdValueDates.Select(date => new Date(date)).ToList();
            _allDates.Add(_valueDate);
            _allTrades = new List<IProduct>();
            _originalTrades = new List<int>();
            _postExerciseTrades = new Dictionary<int, List<int>>();

            // Add the original trades
            var counter = 0;
            foreach (var product in portfolioIn)
            {
                _allTrades.Add(product.Clone());
                _originalTrades.Add(counter);
                if (product is IProductWithEarlyExercise option)
                {
                    var subList = new List<int>();
                    _postExerciseTrades.Add(counter, subList);
                    counter++;
                    _allDates.AddRange(option.GetExerciseDates());
                    var postExProducts = option.GetPostExProducts();
                    foreach (var postExProduct in postExProducts)
                    {
                        _allTrades.Add(postExProduct);
                        subList.Add(counter);
                        counter++;
                    }
                }
                else
                {
                    counter++;
                }
            }

            _allDates = _allDates.Distinct().ToList();
            _allDates.Sort();
        }

        /// <summary>
        /// Replaces the no exercise cashflows for the product at position <paramref name="key"/> with the cashflows based on 
        /// an estimated optimal exercise policy.
        /// </summary>
        /// <param name="key">The position in <see cref="_allTrades"/> of the product to be updated.</param>
        private void ApplyEarlyExercise(int key)
        {
            // Get path-wise regressed values of post exercise products.
            var option = _allTrades[key] as IProductWithEarlyExercise;
            var exDates = option.GetExerciseDates();
            var postExRegressedValues = new double[_numberOfPaths, exDates.Count];
            for (var i = 0; i < exDates.Count; i++)
            {
                // perform regression on each exercise date                
                var postExerciseProductInd = _postExerciseTrades[key][option.GetPostExProductAtDate(exDates[i])];
                var fitted = PerformRegression(exDates[i], _simulatedCFs, _simulatedRegs,
                    new List<int> {postExerciseProductInd});
                postExRegressedValues.SetColumn(i, fitted);
            }

            // Iterate backwards
            // initially the stopping time on all paths is infinity (actually the year 3000)
            var optimalStop = new Date[_numberOfPaths];
            var finalDate = new Date(3000, 1, 1);
            for (var i = 0; i < _numberOfPaths; i++) optimalStop[i] = finalDate;

            for (var exDateCount = exDates.Count - 1; exDateCount >= 0; exDateCount--)
            {
                var exDate = exDates[exDateCount];

                // Optimal flows are underlying product up to the stopping time, then the post exercise product flows afterwards
                var pvOptimalCFs = Vector.Zeros(_numberOfPaths);
                for (var pathCount = 0; pathCount < _numberOfPaths; pathCount++)
                {
                    if (optimalStop[pathCount] < finalDate)
                    {
                        var exProductInd =
                            _postExerciseTrades[key][option.GetPostExProductAtDate(optimalStop[pathCount])];
                        foreach (var cf in _simulatedCFs.GetCFs(exProductInd, pathCount))
                            if (cf.Date > optimalStop[pathCount])
                                pvOptimalCFs[pathCount] += cf.Amount;
                    }

                    foreach (var cf in _simulatedCFs.GetCFs(key, pathCount))
                        if (cf.Date > _valueDate && cf.Date <= optimalStop[pathCount])
                            pvOptimalCFs[pathCount] += cf.Amount;
                }

                // update optimal stopping times
                var optimalCV = _simulatedRegs.FitCFs(exDate, pvOptimalCFs);

                for (var pathCount = 0; pathCount < _numberOfPaths; pathCount++)
                    if (option.IsLongOptionality(exDate) &&
                        optimalCV[pathCount] < postExRegressedValues[pathCount, exDateCount])
                        optimalStop[pathCount] = exDate;
                    else if (!option.IsLongOptionality(exDate) &&
                             optimalCV[pathCount] > postExRegressedValues[pathCount, exDateCount])
                        optimalStop[pathCount] = exDate;
            }

            // All stopping times have been found so now we can update the cashflows.  
            // The cashflows are continuation flows up to the exercise date then cashflows from the 
            // exercise product after that.
            var newCFs = new List<Cashflow>[_numberOfPaths];
            for (var pathCount = 0; pathCount < _numberOfPaths; pathCount++)
            {
                newCFs[pathCount] = new List<Cashflow>();
                var exProductInd = _postExerciseTrades[key][option.GetPostExProductAtDate(optimalStop[pathCount])];
                foreach (var cf in _simulatedCFs.GetCFs(exProductInd, pathCount))
                    if (cf.Date > optimalStop[pathCount])
                        newCFs[pathCount].Add(cf);
                foreach (var cf in _simulatedCFs.GetCFs(key, pathCount))
                    if (cf.Date > _valueDate && cf.Date <= optimalStop[pathCount])
                        newCFs[pathCount].Add(cf);
            }

            _simulatedCFs.Update(key, newCFs);
        }


        /// <summary>
        /// Applies the early exercise conditions to all the products that require it.  Calls 
        /// <see cref="ApplyEarlyExercise(int)"/> on a separate thread for each early exercise 
        /// product.
        /// </summary>
        private void ApplyEarlyExercise()
        {
            if (_useThreads)
            {
                var earlyExThreads = new Thread[_postExerciseTrades.Keys.Count];
                var i = 0;
                foreach (var key in _postExerciseTrades.Keys)
                {
                    earlyExThreads[i] = new Thread(
                        () => ApplyEarlyExercise(key)
                    );
                    earlyExThreads[i].Start();
                    i++;
                }

                foreach (var thread in earlyExThreads) thread.Join();
            }
            else
            {
                foreach (var key in _postExerciseTrades.Keys) ApplyEarlyExercise(key);
            }
        }

        /// <summary>
        /// Performs the simulation of all cashflows.  These may need to be adjusted for early exercise.
        /// </summary>
        private void PerformSimulation()
        {
            // Run the simulation in chunks on several threads.
            if (_useThreads && _numberOfPaths >= 1000)
            {
                var simThreads = new Thread[_maxThreads];
                var simChunkSize = (int) Math.Ceiling(_numberOfPaths / (double) _maxThreads);
                for (var i = 0; i < _maxThreads; i++)
                {
                    var start = i * simChunkSize;
                    var end = Math.Min(start + simChunkSize, _numberOfPaths);
                    simThreads[i] = new Thread(
                        () => PerformSimulationChunk(_allTrades, _allDates, _simulatedCFs, _simulatedRegs, start, end)
                    );
                    simThreads[i].Start();
                }

                foreach (var thread in simThreads) thread.Join();
            }
            else
            {
                PerformSimulationChunk(_allTrades, _allDates, _simulatedCFs, _simulatedRegs, 0, _numberOfPaths);
            }
        }


        /// <summary>
        /// Update the conditional forward PVs over a range.
        /// </summary>
        /// <param name="fwdValueDates">The full set of forward value dates.</param>
        /// <param name="startIndex">The start index of the range.  Forward values after this date will be updated.</param>
        /// <param name="endIndex">The end index of the range.  forward values strictly before this date will be updated.</param>
        /// <param name="regressedValues">The full set of estimated forward values.</param>
        private void ApplyForwardValueRegressionsChunk(List<Date> fwdValueDates, int startIndex, int endIndex,
            double[,] regressedValues)
        {
            for (var i = startIndex; i < endIndex; i++)
            {
                // perform regression on each forward date
                var fitted = PerformRegression(fwdValueDates[i], _simulatedCFs, _simulatedRegs, _originalTrades);
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
            var regressedValues = new double[_numberOfPaths, fwdValueDates.Count];

            if (fwdValueDates.Count >= 20 && _useThreads)
            {
                var regressThreads = new Thread[_maxThreads];
                var regressChunkSize = (int) Math.Ceiling(fwdValueDates.Count / (double) _maxThreads);
                for (var i = 0; i < _maxThreads; i++)
                {
                    var start = i * regressChunkSize;
                    var end = Math.Min(start + regressChunkSize, fwdValueDates.Count);
                    regressThreads[i] = new Thread(
                        () => ApplyForwardValueRegressionsChunk(fwdValueDates, start, end, regressedValues)
                    );
                    regressThreads[i].Start();
                }

                foreach (var thread in regressThreads) thread.Join();
            }
            else
            {
                ApplyForwardValueRegressionsChunk(fwdValueDates, 0, fwdValueDates.Count, regressedValues);
            }

            return regressedValues;
        }

        /// <summary>
        /// Performs the regression for a single date
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="simulatedCFs">The simulated cashflows.</param>
        /// <param name="simulatedRegs">The simulated regs.</param>
        /// <param name="subPortfolio">The sub portfolio.</param>
        /// <returns></returns>
        private double[] PerformRegression(Date date, SimulatedCashflows simulatedCFs,
            SimulatedRegressors simulatedRegs,
            List<int> subPortfolio)
        {
            var cfs = simulatedCFs.GetPathwisePV(date, subPortfolio);
            var fitted = simulatedRegs.FitCFs(date, cfs);
            return fitted;
        }

        private int GetNumberOfUnderlyingFactors()
        {
            // run one simulation to see how many independent variables each simulator provides
            // clone the simulators and portfolio if this is running multi threaded
            var localPortfolio = _allTrades.Clone();

            var (localNumeraire, localSimulators) = GetCopyOfSimulators();
            var mappedSimulators = AssociateFactorsWithSimulators(localPortfolio, localNumeraire, localSimulators);
            PrepareSimulators(_valueDate, localPortfolio, _allDates, localNumeraire, mappedSimulators,
                localSimulators);

            var regressorCount = 0;
            foreach (var simulator in localSimulators)
            {
                simulator.RunSimulation(0);
                regressorCount += simulator.GetUnderlyingFactors(_allDates[0]).Length;
            }

            return regressorCount;
        }


        /// <summary>
        /// Initializes the Coordinator, runs the simulation, applies early exercise rules, 
        /// derives forward value paths.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        private void CalculateAll(IProduct[] portfolioIn, Date valueDate, Date[] fwdValueDates)
        {
            _valueDate = valueDate;
            PreparePortfolios(portfolioIn, fwdValueDates);
            _simulatedCFs =
                new SimulatedCashflows(_allTrades.Count,
                    _numberOfPaths); // initialized outside to allow multiple threads.
            _simulatedRegs = new SimulatedRegressors(_allDates, _numberOfPaths, GetNumberOfUnderlyingFactors());
            PerformSimulation();
            ApplyEarlyExercise();
            if (fwdValueDates.Length > 0) // Only regress if forward values are required.
                _regressedValues = ApplyForwardValueRegressions(fwdValueDates.ToList());
        }


        /// <summary>
        /// Gets all the data that might be of interest after a simulation.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        public ResultStore GetValuePaths(IProduct[] portfolioIn, Date valueDate, Date[] fwdValueDates)
        {
            CalculateAll(portfolioIn, valueDate, fwdValueDates);
            var results = new ResultStore();
            results.Add("regressedFwdsPVs", _regressedValues);
            var fwdCashflowPVs = new double[_numberOfPaths, fwdValueDates.Count()];
            for (var i = 0; i < fwdValueDates.Length; i++)
                fwdCashflowPVs.SetColumn(i, _simulatedCFs.GetPathwisePV(fwdValueDates[i], _originalTrades));
            results.Add("fwdCashflowPVs", fwdCashflowPVs);

            for (var regressorNumber = 0; regressorNumber < _simulatedRegs.GetNumberOfRegressors(); regressorNumber++)
                results.Add("regressor" + regressorNumber,
                    _simulatedRegs.GetRegressors(regressorNumber, fwdValueDates));
            return results;
        }


        /// <summary>
        /// Calculate the expected positive exposure on a portfolio.
        /// </summary>
        /// <param name="portfolioIn">The portfolio in.</param>
        /// <param name="valueDate">The value date.</param>
        /// <param name="fwdValueDates">The forward value dates.</param>
        /// <returns></returns>
        public double[] EPE(IProduct[] portfolioIn, Date valueDate, Date[] fwdValueDates)
        {
            CalculateAll(portfolioIn, valueDate, fwdValueDates);

            var epe = Vector.Zeros(fwdValueDates.Length);

            //Debug.WriteToFile(@"c:\dev\temp\regressedValues.csv", regressedValues);            
            for (var row = 0; row < _regressedValues.GetLength(0); row++)
            for (var col = 0; col < _regressedValues.GetLength(1); col++)
                epe[col] += Math.Max(0, _regressedValues[row, col]);
            for (var col = 0; col < _regressedValues.GetLength(1); col++) epe[col] /= _numberOfPaths;
            return epe;
        }

        public double[,] PFE(IProduct[] portfolioIn, Date valueDate, Date[] fwdValueDates, double[] percentiles)
        {
            CalculateAll(portfolioIn, valueDate, fwdValueDates);

            var pfe = new double[fwdValueDates.Length, percentiles.Length];

            for (var col = 0; col < _regressedValues.GetLength(1); col++)
            {
                var xDist = new EmpiricalDistribution(_regressedValues.GetColumn(col));
                for (var percCount = 0; percCount < percentiles.Length; percCount++)
                    pfe[col, percCount] = xDist.InverseDistributionFunction(percentiles[percCount]);
            }

            return pfe;
        }


        /// <summary>
        /// Values a single specified product
        /// </summary>
        /// <param name="product">The product to be valued.</param>
        /// <param name="valueDate">The value date.</param>
        /// <returns></returns>
        public double Value(IProduct product, Date valueDate)
        {
            return Value(new[] {product}, valueDate);
        }


        /// <summary>
        /// Values the specified portfolio.
        /// </summary>
        /// <param name="portfolioIn">The portfolio to be valued.</param>
        /// <param name="valueDate">The value date.</param>
        /// <returns></returns>
        public double Value(IProduct[] portfolioIn, Date valueDate)
        {
            CalculateAll(portfolioIn, valueDate, new Date[0]);
            var pathwisePVs = _simulatedCFs.GetPathwisePV(valueDate, _originalTrades);
            return pathwisePVs.Average();
        }


        /// <summary>
        /// Initializes the simulators by telling them at which dates they will need to provide which market indices.
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="portfolio"></param>
        /// <param name="fwdDates">Extra dates over and above the contract dates where the simulators will need to 
        /// provide their indices.  This is likely to be forward values required in EPE and PFE profiles.</param>
        /// <param name="numeraireSimulator"></param>
        /// <param name="mappedSimulators"></param>
        /// <param name="availableSimulators"></param>
        private static void PrepareSimulators(Date valueDate, List<IProduct> portfolio, List<Date> fwdDates,
            NumeraireSimulator numeraireSimulator, Dictionary<MarketObservable, Simulator> mappedSimulators,
            List<Simulator> availableSimulators)
        {
            // Reset all the simulators
            foreach (var simulator in availableSimulators) simulator.Reset();

            // Set up the simulators for the times at which they will be queried
            foreach (var product in portfolio)
            {
                product.SetValueDate(valueDate);
                // Tell the simulators at what times indices will be required.
                foreach (var index in product.GetRequiredIndices())
                {
                    var requiredTimes = product.GetRequiredIndexDates(index);
                    mappedSimulators[index].SetRequiredDates(index, requiredTimes);
                    mappedSimulators[index].SetRequiredDates(index, fwdDates);
                }

                // Tell the numeraire simulator at what times it will be required.
                // Tell the FX simulators at what times they will be required.
                foreach (var ccy in product.GetCashflowCurrencies())
                {
                    var requiredDates = product.GetCashflowDates(ccy);
                    numeraireSimulator.SetNumeraireDates(requiredDates);
                    numeraireSimulator.SetNumeraireDates(fwdDates);
                    if (ccy != numeraireSimulator.GetNumeraireCurrency())
                    {
                        MarketObservable index = new CurrencyPair(ccy, numeraireSimulator.GetNumeraireCurrency());
                        mappedSimulators[index].SetRequiredDates(index, requiredDates);
                        mappedSimulators[index].SetRequiredDates(index, fwdDates);
                    }
                }

                foreach (var simulator in availableSimulators)
                    simulator.Prepare(valueDate);
                numeraireSimulator.Prepare(valueDate);
            }

            // Prepare all the simulators
            foreach (var simulator in availableSimulators) simulator.Prepare(valueDate);
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
        private static Dictionary<MarketObservable, Simulator> AssociateFactorsWithSimulators(
            IEnumerable<IProduct> portfolio,
            NumeraireSimulator numeraireSimulator,
            List<Simulator> availableSimulators)
        {
            // Find which simulator will provide each of the potentially required MarketObservables.
            var mappedSimulators = new Dictionary<MarketObservable, Simulator>();
            var indices = new HashSet<MarketObservable>();
            foreach (var product in portfolio)
            {
                foreach (var index in product.GetRequiredIndices())
                    indices.Add(index);
                foreach (var ccy in product.GetCashflowCurrencies().Where(c => c != numeraireSimulator.GetNumeraireCurrency()))
                    indices.Add(new CurrencyPair(ccy, numeraireSimulator.GetNumeraireCurrency()));
            }

            foreach (var index in indices)
            {
                if (mappedSimulators.ContainsKey(index)) continue;
                var found = false;
                foreach (var sim in availableSimulators)
                {
                    if (!sim.ProvidesIndex(index)) continue;
                    if (!found)
                    {
                        mappedSimulators[index] = sim;
                        found = true;
                    }
                    else
                    {
                        throw new ArgumentException(index + " is provided by more than one simulator.");
                    }
                }

                if (found) continue;
                throw new IndexOutOfRangeException("Required index: " + index +
                                                   " is not provided by any of the simulators.");
            }

            return mappedSimulators;
        }
    }
}