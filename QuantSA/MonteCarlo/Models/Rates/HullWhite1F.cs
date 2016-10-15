using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;

namespace MonteCarlo
{
    public class HullWhite1F : NumeraireSimulator
    {
        public delegate double MarketForwards(Date date);
        public delegate double MarketBonds(Date date);

        private double a; // mean reversion
        private double vol;
        private double r0;
        private double rate;
        private Date time0;
        private Currency currency;
        private Dictionary<MarketObservable, Tenor> forecastTenors;
        private MarketForwards fM;
        private MarketBonds PM;

        private List<Date> allDates;
        private double[] allDatesDouble;
        private double[] r;
        private double[] bankAccount;

        public HullWhite1F(double a, double vol, double r0, double rate, Date time0)
        {
            fM = date => 0.07;
            PM = date => Math.Exp(-rate * (date - time0) / 365.0);
            this.a = a;
            this.vol = vol;
            this.r0 = r0;
            this.rate = rate;
            this.time0 = time0;
            forecastTenors = new Dictionary<MarketObservable, Tenor>();
            currency = Currency.ZAR;
        }

        public void AddForecast(FloatingIndex index)
        {
            forecastTenors.Add(index, index.tenor);
        }

        private double theta(Date date)
        {
            double t = (date - time0) / 365.0;
            return a * fM(date) + (vol * vol / (2 * a)) * (1 - Math.Exp(-2 * a * t));
        }


        /// <summary>
        /// Forward zero coupon bond price between <paramref name="date1"/> and <paramref name="date2"/> given
        /// that <paramref name="r"/> has been observed at <paramref name="date1"/>
        /// </summary>
        /// <param name="r"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        private double BondPrice(double r, Date date1, Date date2)
        {            
            double B = (1 / a) * (1 - Math.Exp(-a * (date2 - date1) / 365.0));
            double A = PM(date2) / PM(date1);
            A *= Math.Exp(B * fM(date1) - ((vol * vol) / (4 * a)) * (1 - Math.Exp(-2 * a * (date1 - time0) / 365.0))*B*B);
            return A * Math.Exp(-B * r);           
        }

        public override void Reset()
        {
            allDates = new List<Date>();
        }
        
        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            allDates.AddRange(requiredDates);            
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            allDates.AddRange(requiredDates);
        }

        /// <summary>
        /// Add extra dates to make sure that the minimum spacing is not too large to make the Monte Carlo errors bad.
        /// </summary>
        public override void Prepare()
        {
            double minStepSize = 20;
            allDates.Insert(0, time0);
            allDates = allDates.Distinct().ToList<Date>();
            allDates.Sort();
            List<Date> newDates = new List<Date>();
            newDates.Add(allDates[0]);
            for (int i = 1; i < allDates.Count; i++)
            {
                int nSteps = (int)Math.Floor((allDates[i] - allDates[i - 1]) / minStepSize);
                int days = (allDates[i] - allDates[i - 1]) / (nSteps+1);
                for (int j = 0; j< nSteps; j++)
                {
                    newDates.Add(new Date(allDates[i-1].AddTenor(Tenor.Days((j+1)*days))));
                }
                newDates.Add(allDates[i]);
            }
            allDates = newDates;            
            allDatesDouble = allDates.Select(date => (double)date).ToArray();            
        }

        public override void RunSimulation(int simNumber)
        {
            NormalDistribution dist = new NormalDistribution();
            double[] W = dist.Generate(allDates.Count-1);
            r = new double[allDates.Count];
            bankAccount = new double[allDates.Count];
            r[0] = r0;
            bankAccount[0] = 1;
            for (int i = 0; i< allDates.Count - 1; i++)
            {
                double dt = (allDates[i + 1] - allDates[i]) / 365.0;
                r[i + 1] = r[i] + (theta(allDates[i + 1]) - a * r[i])*dt + vol * Math.Sqrt(dt) * W[i];
                bankAccount[i + 1] = bankAccount[i] * Math.Exp(r[i] * dt);
            }
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            double[] result = new double[requiredDates.Count];
            for (int i = 0; i < requiredDates.Count; i++)
            {
                double rt = Tools.Interpolate1D(requiredDates[i].value, allDatesDouble, r, r[0], r[r.Length-1]);
                Tenor tenor = forecastTenors[index];
                Date date2 = requiredDates[i].AddTenor(tenor);
                double bondPrice = BondPrice(rt, requiredDates[i], date2);
                double rate = 365.0 * (1 / bondPrice - 1) / (date2 - requiredDates[i]);
                result[i] = rate;
            }
            return result;
        }

        public override Currency GetNumeraireCurrency()
        {
            return currency;
        }

        public override double Numeraire(Date valueDate)
        {
            if (valueDate< time0)
            {
                throw new ArgumentException("Numeraire requested at: " + valueDate.ToString() + " but model only starts at " + time0.ToString());
            }
            if (valueDate == time0) return 1.0;
            return Tools.Interpolate1D(valueDate, allDatesDouble, bankAccount, 0, 1);
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            return forecastTenors.ContainsKey(index);
        }

    }
}
