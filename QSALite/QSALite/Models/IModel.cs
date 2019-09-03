using System.Collections.Generic;
using QSALite.Dates;

namespace QSALite
{
    public interface IModel
    {
        bool ProvidesIndex(IMarketObservable marketObservable);
        void RunSimulation(int number, int totalSims);

        List<double> GetRegressors(Date date);
    }
}