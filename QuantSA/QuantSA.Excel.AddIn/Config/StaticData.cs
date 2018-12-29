using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuantSA.ProductExtensions.Data;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;
using QuantSA.Shared.State;

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
            LoadCalendars(sharedData);
            QuantSAState.SetSharedData(sharedData);
        }

        /// <summary>
        /// Construct a Calendar with weekends and public holidays stored in a csv file. see http://www.quantsa.org/latest/Calendar.html
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.FormatException">Encountered date " + str + " which is not in the required format 'yyyy-mm-dd'.</exception>
        private static void LoadCalendars(SharedData sharedData)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "/StaticData/Holidays/";
            var files = Directory.GetFiles(path, "*.csv");
            foreach (var file in files)
            {
                var holidayStrings = File.ReadAllLines(file);
                var holsFromFile = new List<Date>();
                foreach (var str in holidayStrings)
                {
                    var vals = str.Split('-');
                    if (vals.Length == 3)
                    {
                        var date = new Date(int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]));
                        holsFromFile.Add(date);
                    }
                    else
                    {
                        throw new FormatException("Encountered date " + str +
                                                  " which is not in the required format 'yyyy-mm-dd'.");
                    }
                }

                var name = Path.GetFileNameWithoutExtension(file);
                sharedData.Set(new Calendar(name, holsFromFile));
            }
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