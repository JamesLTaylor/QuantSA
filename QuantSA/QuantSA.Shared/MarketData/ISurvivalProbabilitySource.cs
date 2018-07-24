using System;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A an object that can provide survival probabilities for a specific <see cref="ReferenceEntity"/>
    /// </summary>
    
    public abstract class ISurvivalProbabilitySource
    {
        protected Date anchorDate;
        protected ReferenceEntity referenceEntity;

        public ISurvivalProbabilitySource(ReferenceEntity referenceEntity, Date anchorDate)
        {
            this.referenceEntity = referenceEntity;
            this.anchorDate = anchorDate;
        }

        /// <summary>
        /// The earliest date after which survival probabilities can be obtained.
        /// </summary>
        /// <returns></returns>
        public virtual Date getAnchorDate()
        {
            return anchorDate;
        }

        /// <summary>
        /// Gets the survival probability between the anchor date and the date provided.
        /// </summary>
        /// <param name="date">The date up to which the survival probability will be calculated.</param>
        /// <returns></returns>
        public abstract double GetSP(Date date);

        /// <summary>
        /// Gets the survival probability between two dates.  Both dates must be on or after the anchor date.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">startDate must be before endDate.</exception>
        public virtual double GetSP(Date startDate, Date endDate)
        {
            if (startDate > endDate) throw new ArgumentException("startDate must be before endDate.");
            return GetSP(endDate) / GetSP(startDate);
        }


        public ReferenceEntity GetReferenceEntity()
        {
            return referenceEntity;
        }
    }
}