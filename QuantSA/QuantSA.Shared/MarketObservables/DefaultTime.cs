namespace QuantSA.Shared.MarketObservables
{
    /// <summary>
    /// The observed default time of a reference entity.  If default has no taken place
    /// it should have the value of 1 Jan 3000.
    /// </summary>
    /// <seealso cref="MarketObservable" />
    public class DefaultTime : MarketObservable
    {
        public ReferenceEntity RefEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTime"/> class.
        /// </summary>
        /// <param name="refEntity">The reference entity for whom the default time will be monitored.</param>
        public DefaultTime(ReferenceEntity refEntity)
        {
            RefEntity = refEntity;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.  Used to compare <see cref="MarketObservable"/>s.  
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "DEFAULT:TIME:" + RefEntity;
        }
    }
}