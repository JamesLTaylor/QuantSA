using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// The observed default time of a reference entity.  If default has no taken place
    /// it should have the value of 1 Jan 3000.
    /// </summary>
    /// <seealso cref="QuantSA.General.MarketObservable" />
    [Serializable]
    public class DefaultTime : MarketObservable
    {
        private ReferenceEntity refEntity;
        private string toString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTime"/> class.
        /// </summary>
        /// <param name="refEntity">The reference entity for whom the default time will be monitored.</param>
        public DefaultTime(ReferenceEntity refEntity)
        {
            this.refEntity = refEntity;
            toString = "DEFAULT:TIME:" + refEntity.ToString().ToUpper();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.  Used to compare <see cref="MarketObservable"/>s.  
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return toString;
        }
    }
}
