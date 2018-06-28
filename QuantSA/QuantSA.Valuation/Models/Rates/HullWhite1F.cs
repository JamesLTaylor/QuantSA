using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Random;
using Accord.Statistics.Distributions.Univariate;
using QuantSA.General;
using QuantSA.General.Dates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation
{
    /// <summary>
    /// A single factor Hull White simulator.  It can simulate a numeraire and any number of
    /// forward rates off the same curve.
    /// </summary>
    /// <seealso cref="QuantSA.Valuation.NumeraireSimulator" />
    public class HullWhite1F : NumeraireSimulator
    {
        public delegate double MarketBonds(Date date);

        public delegate double MarketForwards(Date date);

        private readonly double a; // mean reversion

        private List<Date> allDates;
        private double[] allDatesDouble;
        private double[] bankAccount;
        private readonly Currency currency;
        private readonly MarketForwards fM;
        private Dictionary<MarketObservable, Tenor> forecastTenors;
        private readonly MarketBonds PM;
        private double[] r;
        private readonly double r0;
        private readonly double rate;
        private readonly Date time0;
        private readonly double vol;

        public HullWhite1F(Currency currency, double a, double vol, double r0, double rate, Date time0)
        {
            fM = date => rate;
            PM = date => Math.Exp(-rate * (date - time0) / 365.0);
            this.a = a;
            this.vol = vol;
            this.r0 = r0;
            this.rate = rate;
            this.time0 = time0;
            forecastTenors = new Dictionary<MarketObservable, Tenor>();
            this.currency = currency;
        }

        public void AddForecast(FloatRateIndex index)
        {
            forecastTenors.Add(index, index.tenor);
        }

        private double theta(Date date)
        {
            var t = (date - time0) / 365.0;
            return a * fM(date) + vol * vol / (2 * a) * (1 - Math.Exp(-2 * a * t));
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
            // Equation 3.39 in Brigo Mercurio 2nd edition:
            var T = (date2 - time0) / 365.0;
            var t = (date1 - time0) / 365.0;
            var B = 1 / a * (1 - Math.Exp(-a * (T - t)));
            var A = PM(date2) / PM(date1);
            A *= Math.Exp(B * fM(date1) - vol * vol / (4 * a) * (1 - Math.Exp(-2 * a * t)) * B * B);
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
        /// <para/>
        /// At this point the dates are all copied.
        /// </summary>
        public override void Prepare()
        {
            double minStepSize = 20;
            allDates.Insert(0, time0);
            allDates = allDates.Distinct().ToList();
            allDates.Sort();
            var newDates = new List<Date>();
            newDates.Add(new Date(allDates[0]));
            for (var i = 1; i < allDates.Count; i++)
            {
                var nSteps = (int) Math.Floor((allDates[i] - allDates[i - 1]) / minStepSize);
                var days = (allDates[i] - allDates[i - 1]) / (nSteps + 1);
                for (var j = 0; j < nSteps; j++)
                    newDates.Add(new Date(allDates[i - 1].AddTenor(Tenor.FromDays((j + 1) * days))));
                newDates.Add(new Date(allDates[i]));
            }

            allDates = newDates;
            allDatesDouble = allDates.Select(date => (double) date).ToArray();
        }

        public override void RunSimulation(int simNumber)
        {
            var dist = new NormalDistribution();
            Generator.Seed = -1585814591 * simNumber; // This magic number is: "HW1FSimulator".GetHashCode();
            var W = dist.Generate(allDates.Count - 1);
            r = new double[allDates.Count];
            bankAccount = new double[allDates.Count];
            r[0] = r0;
            bankAccount[0] = 1;
            for (var i = 0; i < allDates.Count - 1; i++)
            {
                var dt = (allDates[i + 1] - allDates[i]) / 365.0;
                r[i + 1] = r[i] + (theta(allDates[i + 1]) - a * r[i]) * dt + vol * Math.Sqrt(dt) * W[i];
                bankAccount[i + 1] = bankAccount[i] * Math.Exp(r[i] * dt);
            }
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            var result = new double[requiredDates.Count];
            for (var i = 0; i < requiredDates.Count; i++)
            {
                var rt = Tools.Interpolate1D(requiredDates[i].value, allDatesDouble, r, r[0], r[r.Length - 1]);
                var tenor = forecastTenors[index];
                var date2 = requiredDates[i].AddTenor(tenor);
                var bondPrice = BondPrice(rt, requiredDates[i], date2);
                var rate = 365.0 * (1 / bondPrice - 1) / (date2 - requiredDates[i]);
                result[i] = rate;
            }

            return result;
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            var rt = Tools.Interpolate1D(date.value, allDatesDouble, r, r[0], r[r.Length - 1]);
            return new[] {rt};
        }

        public override Currency GetNumeraireCurrency()
        {
            return currency;
        }

        public override double Numeraire(Date valueDate)
        {
            if (valueDate < time0)
                throw new ArgumentException("Numeraire requested at: " + valueDate + " but model only starts at " +
                                            time0);
            if (valueDate == time0) return 1.0;
            return Tools.Interpolate1D(valueDate, allDatesDouble, bankAccount, 1, bankAccount.Last());
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            return forecastTenors.ContainsKey(index);
        }
    }
}