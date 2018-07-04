using QuantSA.ProductExtensions.Data;
using QuantSA.Shared.Serialization;

namespace QuantSA.Excel.Addin.Config
{
    public class StaticData
    {
        public void Load()
        {
            var sharedData = new SharedData();
            QuantSAState.SetSharedData(sharedData);
        }
    }
}