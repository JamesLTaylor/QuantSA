using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation
{
    /// <summary>
    /// Collection of cashflows, stored by product number and path number
    /// </summary>
    public class SimulatedCashflows
    {
        private readonly List<Cashflow>[][] _allCFs;
        private readonly int _nSims;

        public SimulatedCashflows(int productCount, int nSims)
        {
            _nSims = nSims;
            _allCFs = new List<Cashflow>[productCount][];
            for (var i = 0; i < productCount; i++)
            {
                _allCFs[i] = new List<Cashflow>[nSims];
                for (var j = 0; j < nSims; j++) _allCFs[i][j] = new List<Cashflow>();
            }
        }

        /// <summary>
        /// Adds a cashflow for the given product number and simulation number
        /// </summary>
        /// <param name="productNumber">The product number.</param>
        /// <param name="pathNumber">The path number.</param>
        /// <param name="cf">The cashflow.</param>
        public void Add(int productNumber, int pathNumber, Cashflow cf)
        {
            _allCFs[productNumber][pathNumber].Add(cf);
        }

        /// <summary>
        /// Sums the cashflows associated with the subportfolio that take place strictly after the provided date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="subPortfolio">The sub portfolio.</param>
        /// <returns></returns>
        internal double[] GetPathwisePV(Date date, List<int> subPortfolio)
        {
            //TODO: This could be done faster for a set of dates if the cashflows are ordered by time.  Then the value in column i is the value in column i+1 plus the cashflows that take place between the dates associated with the columns.
            var result = new double[_nSims];
            foreach (var productCounter in subPortfolio)
                for (var pathCounter = 0; pathCounter < _nSims; pathCounter++)
                    foreach (var cf in _allCFs[productCounter][pathCounter])
                        if (cf.Date > date)
                            result[pathCounter] += cf.Amount;
            return result;
        }


        internal List<Cashflow> GetCFs(int productNumber, int pathNumber)
        {
            return _allCFs[productNumber][pathNumber];
        }

        internal void Update(int productNumber, List<Cashflow>[] newCFs)
        {
            _allCFs[productNumber] = newCFs;
        }
    }
}