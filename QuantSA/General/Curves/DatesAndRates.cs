using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A collection of dates and rates for interpolating.  The rates can be used as continuously compounded rates to get 
    /// discount factors or interpolated directly.
    /// </summary>
    [Serializable]    
    public class DatesAndRates : IDiscountingSource, ICurve
    {
        //TODO: Separate this class into one that discounts and one that interpolates.  It could be abused/misused in its current form
        private double anchorDateValue;
        private Date anchorDate;
        private double[] dates;
        private double[] rates;
        private Currency currency;
        
        /// <summary>
        /// Creates a curve.  The curve will be flat from the anchor date to the first date and from the last date in dates until maximumDate
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="anchorDate">Date from which the curve applies.  Interpolation before this date won't work.</param>
        /// <param name="dates">Must be sorted in increasing order.</param>
        /// <param name="rates">The rates.  If the curve is going to be used to supply discount factors then these rates must be continuously compounded.</param>
        /// <param name="maximumDate">The date beyond which interpolation will not be allowed.  If it is null or left out then the last date in dates will be used.</param>
        public DatesAndRates(Currency currency, Date anchorDate, Date[] dates, double[] rates, Date maximumDate=null)
        {
            this.anchorDate = anchorDate;
            for (int i = 1; i<dates.Length; i++)
            {
                if (dates[i].value <= dates[i - 1].value) throw new ArgumentException("Dates must be strictly increasing");
            }
            List<double> datesList = new List<double>(dates.GetValues());
            List<double> ratesList = new List<double>(rates.Clone() as double[]);
            if (dates[0]>anchorDate)
            {
                datesList.Insert(0, anchorDate.value);
                ratesList.Insert(0, rates[0]);                            
            }
            if (maximumDate!=null)
            {
                datesList.Add(maximumDate);
                ratesList.Add(rates.Last());
            }
            this.anchorDateValue = anchorDate;
            this.dates = datesList.ToArray();
            this.rates = ratesList.ToArray();
            this.currency = currency;
        }

        public Date GetAnchorDate()
        {
            return anchorDate;
        }

        /// <summary>
        /// Get a discount factor assuming the rates are continuosly compounded and the daycount in actual/365
        /// </summary>
        /// <param name="date">The date at which the discount factor (DF) is required.  The DF will apply from the anchor date to this date.</param>
        /// <returns></returns>
        public double GetDF(Date date)
        {
            double rate = InterpAtDate(date);
            double df = Math.Exp(-rate * (date - anchorDateValue) / 365.0);
            return df;
        }


        /// <summary>
        /// Interpolate the curve.
        /// </summary>
        /// <param name="date">The date at which the rate is required.</param>
        /// <returns></returns>
        public double InterpAtDate(Date date)
        {
            if (date.value < anchorDateValue) throw new ArgumentException("Interpolation date  (" + date.ToString() + ") is before the anchor date of the curve.(" + (new Date(anchorDateValue)).ToString() + ")");
            if (date.value > dates.Last()) throw new ArgumentException("Interpolation date (" + date.ToString() + ") is after the last date on the curve.(" + (new Date(dates.Last())).ToString() +")");
            LinearSpline spline = LinearSpline.InterpolateSorted(dates, rates);
            return spline.Interpolate(date);            
        }

        public Currency GetCurrency()
        {
            return currency;
        }
    }
}
