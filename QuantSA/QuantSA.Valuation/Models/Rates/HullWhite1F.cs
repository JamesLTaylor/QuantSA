using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Random;
using Accord.Statistics.Distributions.Univariate;
using Newtonsoft.Json;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation.Models.Rates
{
    public delegate double MarketBonds(Date date);

    public delegate double MarketForwards(Date date);

    /// <summary>
    /// A single factor Hull White simulator.  It can simulate a numeraire and any number of
    /// forward rates off the same curve.
    /// </summary>
    /// <seealso cref="QuantSA.Valuation.NumeraireSimulator" />
    public class HullWhite1F : NumeraireSimulator
    {
        private List<FloatRateIndex> _floatRateIndices;
        private readonly double _inputRate;
        private readonly double a; // mean reversion
        private readonly Currency currency;
        private readonly double r0;
        private readonly double vol;

        [JsonIgnore] private Date _anchorDate;
        [JsonIgnore] private List<Date> allDates;
        [JsonIgnore] private double[] allDatesDouble;
        [JsonIgnore] private double[] bankAccount;
        [JsonIgnore] private MarketForwards fM;
        [JsonIgnore] private MarketBonds PM;
        [JsonIgnore] private double[] r;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="a"></param>
        /// <param name="vol"></param>
        /// <param name="r0"></param>
        /// <param name="inputRate">the flat continuously compounded rate that is fitted to.</param>
        /// <param name="floatRateIndices"></param>
        public HullWhite1F(Currency currency, double a, double vol, double r0, double inputRate,
            IEnumerable<FloatRateIndex> floatRateIndices = null)
        {
            this.a = a;
            this.vol = vol;
            this.r0 = r0;
            _inputRate = inputRate;
            _floatRateIndices = floatRateIndices != null ? floatRateIndices.ToList() : new List<FloatRateIndex>();
            this.currency = currency;
        }

        private double Theta(Date date)
        {
            var t = (date - _anchorDate) / 365.0;
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
            var T = (date2 - _anchorDate) / 365.0;
            var t = (date1 - _anchorDate) / 365.0;
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
            if (allDates == null) allDates = requiredDates;
            else
                allDates.AddRange(requiredDates);
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            if (allDates == null) allDates = requiredDates;
            else
                allDates.AddRange(requiredDates);
        }

        /// <summary>
        /// Add extra dates to make sure that the minimum spacing is not too large to make the Monte Carlo errors bad.
        /// <para/>
        /// At this point the dates are all copied.
        /// </summary>
        public override void Prepare(Date anchorDate)
        {
            _anchorDate = anchorDate;
            fM = date => _inputRate;
            PM = date => Math.Exp(-_inputRate * (date - anchorDate) / 365.0);
            double minStepSize = 20;
            allDates.Insert(0, anchorDate);
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
                r[i + 1] = r[i] + (Theta(allDates[i + 1]) - a * r[i]) * dt + vol * Math.Sqrt(dt) * W[i];
                bankAccount[i + 1] = bankAccount[i] * Math.Exp(r[i] * dt);
            }
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            var floatRateIndex = index as FloatRateIndex;
            var result = new double[requiredDates.Count];
            for (var i = 0; i < requiredDates.Count; i++)
            {
                var rt = Tools.Interpolate1D(requiredDates[i].value, allDatesDouble, r, r[0], r[r.Length - 1]);
                var tenor = floatRateIndex.Tenor;
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
            if (valueDate < _anchorDate)
                throw new ArgumentException(
                    $"Numeraire requested at: {valueDate} but model only starts at {_anchorDate}");
            if (valueDate == _anchorDate) return 1.0;
            return Tools.Interpolate1D(valueDate, allDatesDouble, bankAccount, 1, bankAccount.Last());
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            return _floatRateIndices.Contains(index);
        }

        public void AddForecast(FloatRateIndex index)
        {
            if (_floatRateIndices == null) _floatRateIndices = new List<FloatRateIndex>();
            _floatRateIndices.Add(index);
        }
    }
}