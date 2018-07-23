using System;
using System.IO;
using System.Linq;
using QuantSA.ProductExtensions.Data;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;

namespace QuantSA.Excel.Addin.Config
{
    public class StaticData
    {
        public static void Load()
        {
            var sharedData = new SharedData();
            LoadCurrencies(sharedData);
            LoadCurrencyPairs(sharedData);
            LoadFloatRateIndices(sharedData);
            QuantSAState.SetSharedData(sharedData);
        }

        private static void LoadFloatRateIndices(SharedData sharedData)
        {
            var filename = AppDomain.CurrentDomain.BaseDirectory + "/StaticData/FloatRateIndices.csv";
            var lines = File.ReadAllLines(filename);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 4)
                    throw new ArgumentException($"File must have at least 4 columns. {filename}");
                sharedData.Set(new FloatRateIndex(parts[0], sharedData.Get<Currency>(parts[1]),
                    parts[2], new Tenor(parts[3])));
            }
        }

        private static void LoadCurrencies(SharedData sharedData)
        {
            var filename = AppDomain.CurrentDomain.BaseDirectory + "/StaticData/Currencies.csv";
            var lines = File.ReadAllLines(filename);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 4)
                    throw new ArgumentException($"File must have at least 4 columns. {filename}");
                var ccyName = parts[0];
                var isDefault = ccyName.Contains('*');
                ccyName = ccyName.Replace("*", "");
                sharedData.Set(new Currency(ccyName));
            }
        }

        private static void LoadCurrencyPairs(SharedData sharedData)
        {
            var filename = AppDomain.CurrentDomain.BaseDirectory + "/StaticData/CurrencyPairs.csv";
            var lines = File.ReadAllLines(filename);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 3)
                    throw new ArgumentException($"File must have at least 3 columns. {filename}");
                sharedData.Set(new CurrencyPair(parts[0], sharedData.Get<Currency>(parts[1]),
                    sharedData.Get<Currency>(parts[2])));
            }
        }
    }
}