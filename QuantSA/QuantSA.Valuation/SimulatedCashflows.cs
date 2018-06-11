using Accord.Math;
using QuantSA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation
{
    /// <summary>
    /// Collection of cashflows, stored by product number and path number
    /// </summary>
    public class SimulatedCashflows
    {        
        public List<Cashflow>[][] allCFs;
        private int nSims;

        public SimulatedCashflows(int productCount, int nSims)
        {            
            this.nSims = nSims;
            allCFs = new List<Cashflow>[productCount][];
            for (int i = 0; i < productCount; i++)
            {
                allCFs[i] = new List<Cashflow>[nSims];
                for (int j = 0; j< nSims; j++)
                {
                    allCFs[i][j] = new List<Cashflow>();
                }
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
            allCFs[productNumber][pathNumber].Add(cf);
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
            double[] result = Vector.Zeros(nSims);
            foreach (int productCounter in subPortfolio)
            {
                for (int pathCounter = 0; pathCounter < nSims; pathCounter++)
                {
                    foreach (Cashflow cf in allCFs[productCounter][pathCounter])
                    {
                        if (cf.Date > date) result[pathCounter] += cf.Amount;
                    }
                }
            }
            return result;
        }
      

        internal List<Cashflow> GetCFs(int productNumber, int pathNumber)
        {
            return allCFs[productNumber][pathNumber];
        }

        internal void Update(int productNumber, List<Cashflow>[] newCFs)
        {
            allCFs[productNumber] = newCFs;
        }
    }
}
