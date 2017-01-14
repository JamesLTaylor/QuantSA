using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General;
using Accord.Math.Optimization;

namespace QuantSA.Valuation
{
    public class MultiCurveStripper
    {
        private Date valueDate;
        private List<ICurveForStripping> curveSet;
        private Dictionary<ICurveForStripping, List<Date>> curveDates;
        private List<Func<double>> targetMetrics;
        private List<double> targetValues;
        private List<double> targetWeights;

        /// <summary>
        /// The key is the index in the full guess vector while the value is a tuple of curve index in <see cref="curveSet"/> and 
        /// the index in the rates of the that curve.
        /// </summary>
        private Dictionary<int, Tuple<int, int>> curveAndIndexMap;

        

        public MultiCurveStripper(Date valueDate)
        {
            this.valueDate = valueDate;
            curveSet = new List<ICurveForStripping>();
            curveDates = new Dictionary<ICurveForStripping, List<Date>>();
            targetMetrics = new List<Func<double>>();
            targetValues = new List<double>();
            targetWeights = new List<double>();
            curveAndIndexMap = new Dictionary<int, Tuple<int, int>>();
        }


        /// <summary>
        /// Adds the specified product to the curve stripping set.
        /// <para/>
        /// The product will be changed when it is added by its valueDate being set.  The curve itself will 
        /// also be changed when the <see cref="Strip"/> method is called.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="targetMetric">The function that will be evaluated during the optimization.</param>
        /// <param name="targetValue"></param>
        /// <param name="targetWeight"></param>
        /// <param name="curve">The curve.</param>
        public void AddDiscounting<T>(Product product, Func<double> targetMetric, double targetValue, double targetWeight, T curve) 
            where T : IDiscountingSource, ICurveForStripping
        {
            targetMetrics.Add(targetMetric);
            targetValues.Add(targetValue);
            targetWeights.Add(targetWeight);
            product.SetValueDate(valueDate);
            Currency discountCcy = curve.GetCurrency();
            Date lastDate = product.GetCashflowDates(discountCcy).Last();
            if (!curveDates.ContainsKey(curve))
            {
                curveSet.Add(curve);
                curveDates.Add(curve, new List<Date>());
            }
            curveDates[curve].Add(lastDate);
        }
        /// <summary>
        /// Adds the specified product to the curve stripping set.
        /// <para/>
        /// The product will be changed when it is added by its valueDate being set.  The curve itself will 
        /// also be changed when the <see cref="Strip"/> method is called.
        /// </summary>
        /// <typeparam name="CurveType">The type of the curve type that this product will add a node date to.</typeparam>
        /// <param name="product">The product.</param>
        /// <param name="targetMetric">The function that will be evaluated during the optimization.</param>
        /// <param name="targetValue"></param>
        /// <param name="targetWeight"></param>
        /// <param name="curve">The curve to which the node date for this product will be added.</param>
        /// <param name="index">The index that the product depends on and that the curve provides that will be used to find the 
        /// node date.  The node date will be the last date that this index is required.</param>
        public void AddForecast<CurveType>(Product product, Func<double> targetMetric, double targetValue, double targetWeight,
                                           CurveType curve, FloatingIndex index)
            where CurveType : IFloatingRateSource, ICurveForStripping
        {
            targetMetrics.Add(targetMetric);
            targetValues.Add(targetValue);
            targetWeights.Add(targetWeight);
            product.SetValueDate(valueDate);
            Date lastDate = product.GetRequiredIndexDates(index).Last();            
            if (!curveDates.ContainsKey(curve))
            {
                curveSet.Add(curve);
                curveDates.Add(curve, new List<Date>());
            }
            curveDates[curve].Add(lastDate);
        }


        public void Strip()
        {
            // Set the dates for all the curves
            foreach (ICurveForStripping curve in curveDates.Keys)
            {
                curve.SetDates(curveDates[curve].ToArray());
            }

            // Check that all the products can be valued
            double[] values = new double[targetMetrics.Count];
            for (int i = 0; i<targetMetrics.Count; i++)
            {
                values[i] = targetMetrics[i]();
            }

            // Get the vector to be solved
            List<double> guessList = new List<double>();            
            int totalCounter = 0;
            for (int i = 0; i< curveSet.Count; i++)
            {
                double[] rates = curveSet[i].GetRates();
                for (int j=0; j<rates.Length; j++)
                {
                    guessList.Add(rates[j]);
                    curveAndIndexMap[totalCounter] = new Tuple<int, int>(i, j);
                    totalCounter++;
                }
            }
            if (guessList.Count != targetMetrics.Count)
                throw new ArgumentException(string.Format("There are {0} metrics as contraints but the curves have {1} free parameters.", targetMetrics.Count, guessList.Count));

            double[] guess = guessList.ToArray();
            var nm = new NelderMead(numberOfVariables: guess.Length, function: ErrorFunction);
            bool success = nm.Minimize(guess);
            double minValue = nm.Value;
            double[] solution = nm.Solution;
        }

        private double ErrorFunction(double[] x)
        {            
            double error = 0;
            // Update the curves
            for (int i = 0; i < x.Length; i++)
            {
                int curveIndex = curveAndIndexMap[i].Item1;
                int valueIndex = curveAndIndexMap[i].Item2;
                curveSet[curveIndex].SetRate(valueIndex, x[i]);
            }
            // Evaluate the target metrics and construct the error function value
            for (int i = 0; i < targetMetrics.Count; i++)
            {
                double diff = targetMetrics[i]() - targetValues[i];
                error += 1e6 * targetWeights[i] * diff * diff;
            }
            return error;
        }
    }
}
