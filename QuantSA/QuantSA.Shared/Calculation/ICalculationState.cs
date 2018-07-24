namespace QuantSA.Shared.Calculation
{
    public interface ICalculationState
    {
        /// <summary>
        /// Perform a calculation.
        /// </summary>
        /// <returns></returns>
        ResultStore PerformCalc();

        /// <summary>
        /// Convert a complete instance <see cref="ICalculationState"/> into a string.
        /// </summary>
        /// <returns></returns>
        string Serialize();

        /// <summary>
        /// Construct an <see cref="ICalculationState"/> from a message.
        /// </summary>
        /// <param name="message"></param>
        ICalculationState Deserialize(string message);
    }
}