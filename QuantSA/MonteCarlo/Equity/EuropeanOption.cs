using QuantSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo.Equity
{
    public class EuropeanOption : Product
    {
        private Date exerciseDate;
        private double fwdPrice;
        private Share share;
        private double strike;

        public EuropeanOption(string shareCode, double strike, Date exerciseDate)
        {
            share = new Share(shareCode);
            this.strike = strike;
            this.exerciseDate = exerciseDate;
        }

        public override double[,] GetCFs()
        {
            return new double[,] { { exerciseDate, Math.Max(0, fwdPrice-strike) } };
        }

        public override IEnumerable<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> { share };
        }

        public override int[] GetRequiredTimes(Date valueDate, MarketObservable index)
        {
            if (valueDate.date <= exerciseDate.date)
            {
                return new int[] { exerciseDate.value };
            }
            else
            {
                return null;
            }
        }

        public override void SetIndices(MarketObservable index, double[] indices)
        {
            fwdPrice = indices[0];
        }
    }
}
