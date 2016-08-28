using System;
using System.Collections.Generic;

namespace MonteCarlo
{
    public class Product
    {
        internal IEnumerable<MarketObservable> GetRequiredIndices()
        {
            throw new NotImplementedException();
        }

        internal double[] GetRequiredTimes(Date valueDate, MarketObservable key)
        {
            throw new NotImplementedException();
        }

        internal double[,] GetCFs()
        {
            throw new NotImplementedException();
        }

        internal void SetIndices(MarketObservable index, double[] indices)
        {
            throw new NotImplementedException();
        }
    }
}