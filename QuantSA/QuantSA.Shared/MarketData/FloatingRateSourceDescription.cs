using Newtonsoft.Json;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Shared.MarketData
{
    public class FloatingRateSourceDescription : MarketDataDescription<IFloatingRateSource>
    {
        private readonly FloatRateIndex _index;
        private string _name;

        public FloatingRateSourceDescription(FloatRateIndex index)
        {
            _index = index;
        }

        [JsonIgnore] public override string Name
        {
            get
            {
                if (_name != null)
                    return _name;
                _name = $"FloatingRateSource.{_index}";
                return _name;
            }
        }
    }
}