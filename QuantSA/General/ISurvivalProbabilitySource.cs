using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A an object that can provide survival probabilites for a specific <see cref="ReferenceEntity"/>
    /// </summary>
    [Serializable]
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
        /// The earliest date after which survival probabilities obtained.
        /// </summary>
        /// <returns></returns>
        public virtual Date getAnchorDate() { return anchorDate; }

        public abstract double GetSP(Date date);

        public virtual double GetSP(Date startDate, Date endDate) {
            if (startDate > endDate) throw new ArgumentException("startDate must be before endDate.");
            return GetSP(endDate) / GetSP(startDate);
        }

        public ReferenceEntity GetReferenceEntity()
        {
            return referenceEntity;
        }
    }
}
