namespace QSALite.Calculations
{
    public interface ICalculation
    {
        Cube PerformCalculation(ICalculationState calculationState);
    }
}