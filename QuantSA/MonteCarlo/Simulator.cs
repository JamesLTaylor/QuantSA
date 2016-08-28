using System;

namespace MonteCarlo
{
    public class Simulator
    {
        internal bool ProvidesIndex(MarketObservable index)
        {
            throw new NotImplementedException();
        }

        internal void SetRequiredTimes(MarketObservable index, double[] requiredTimes)
        {
            throw new NotImplementedException();
        }

        internal void RunSimulation(int startSim, int endSim)
        {
            throw new NotImplementedException();
        }

        internal double[] GetIndices(MarketObservable index, double[] requiredTimes)
        {
            throw new NotImplementedException();
        }
    }
}