using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General.Conventions.BusinessDay
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// these values are taken from QLNET (https://github.com/amaggiulli/QLNet)
    /// </remarks>
    public enum BusinessDayConvention
    {
        // ISDA
        Following,          /*!< Choose the first business day after
                                 the given holiday. */
        ModifiedFollowing,  /*!< Choose the first business day after
                                 the given holiday unless it belongs
                                 to a different month, in which case
                                 choose the first business day before
                                 the holiday. */
        Preceding,          /*!< Choose the first business day before
                                 the given holiday. */
        // NON ISDA
        ModifiedPreceding,  /*!< Choose the first business day before
                                 the given holiday unless it belongs
                                 to a different month, in which case
                                 choose the first business day after
                                 the holiday. */
        Unadjusted,          /*!< Do not adjust. */
    }
}
