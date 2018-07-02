namespace QuantSA.Shared.MarketObservables
{
    /// <summary>
    /// The recovery rate in a default event.  It is undefined until there is a default.  When it is undefined 
    /// set it to Double.NaN in the simulation, this will make sure that it is not inadvertently used.
    /// </summary>
    public class DefaultRecovery : MarketObservable
    {
        public ReferenceEntity RefEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTime"/> class.
        /// </summary>
        /// <param name="refEntity">The reference entity for whom the default time will be monitored.</param>
        public DefaultRecovery(ReferenceEntity refEntity)
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
            return "DEFAULT:RECOVERYRATE:" + RefEntity;
        }
    }
}