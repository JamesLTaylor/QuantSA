using Accord.Math;
using QuantSA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Valuation
{
    /// <summary>
    /// Collection of cashflows, stored by product number and path number
    /// </summary>
    public class SimulatedCashflows
    {        
        List<Product> products;
        List<Cashflow>[][] allCFs;
        private int nSims;

        public SimulatedCashflows(List<Product> products, int nSims)
        {
            this.products = products;
            this.nSims = nSims;
            allCFs = new List<Cashflow>[products.Count][];
            for (int i = 0; i < products.Count; i++)
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
        internal double[] GetPathwisePV(Date date, List<Product> subPortfolio)
        {
            double[] result = Vector.Zeros(nSims);
            for (int productCounter = 0; productCounter< products.Count; productCounter++)
            {
                if (subPortfolio.Contains(products[productCounter]))
                {
                    for (int pathCounter = 0; pathCounter < nSims; pathCounter++)
                    {
                        foreach (Cashflow cf in allCFs[productCounter][pathCounter])
                        {
                            if (cf.date > date) result[pathCounter] += cf.amount;
                        }
                    }
                }                
            }
            return result;
        }

        internal List<Cashflow> GetCFs(int productNumber, int pathNumber)
        {
            return allCFs[productNumber][pathNumber];
        }
    }
}
