using QuantSA.Core.Primitives;
using QuantSA.Core.Products.Equity;
using QuantSA.Valuation;
using QuantSA.Excel.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.State;
using QuantSA.Valuation.Models.Equity;
using QuantSA.Core.Formulae;
using System;
using QuantSA.CoreExtensions.Products.Equity;


namespace QuantSA.ExcelFunctions
{
    public class XLEquities
    {
        [QuantSAExcelFunction(Description = "Create a model that simulates multiple equities in one currency.  Assumes lognormal dynamics.",
            Name = "QSA.CreateEquityModel",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateEquityModel.html")]
        public static NumeraireSimulator CreateEquityModel([QuantSAExcelArgument(Description = "The discounting curve.  Will be used for discounting and as the drift rate for the equities.")]IDiscountingSource discountCurve,
            [QuantSAExcelArgument(Description = "Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.")]Share[] shares,
            [QuantSAExcelArgument(Description = "The values of all the shares on the anchor date of the discounting curve. ")]double[] spotPrices,
            [QuantSAExcelArgument(Description = "A single volatility for each share.")]double[] volatilities,
            [QuantSAExcelArgument(Description = "A single continuous dividend yield rate for each equity.")]double[] divYields,
            [QuantSAExcelArgument(Description = "A square matrix of correlations between shares, the rows and columns must be in the same order as the shares were listed in shareCodes.")]double[,] correlations,
            [QuantSAExcelArgument(Description = "The floating rate forecast curves for all the rates that the products in the portfolio will need.", Default = null)]IFloatingRateSource[] rateForecastCurves)
        {
            return new EquitySimulator(shares, spotPrices, volatilities, divYields, correlations, discountCurve,
                rateForecastCurves);
        }


        [QuantSAExcelFunction(Description = "Create a model that simulates multiple equities in one currency.  Assumes lognormal dynamics.",
            Name = "QSA.CreateEquityCall",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateEquityCall.html")]
        public static Product CreateEquityCall([QuantSAExcelArgument(Description = "A share.  This needs to match a share in the model that will be used to value this.")]Share share,
            [QuantSAExcelArgument(Description = "Exercise date.")]Date exerciseDate,
            [QuantSAExcelArgument(Description = "Strike")]double strike, 
            [QuantSAExcelArgument(Description = "PutOrCall")] PutOrCall putOrCall)
        {
            return new EuropeanOption(share, putOrCall, strike, exerciseDate);
        }

        [QuantSAExcelFunction(Description = "Create a share object that can be used by products and models and add it to the static data for QuantSA. Acts like " +
                                            "the static data files that add currencies.",
            Name = "QSA.CreateShare",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateShare.html")]
        public static string CreateShare([QuantSAExcelArgument] string shareCode,
            [QuantSAExcelArgument] Currency currency)
        {
            var share = new Share(shareCode, currency);
            QuantSAState.SharedData.Set(share);
            return share.GetName();
        }

        // Black-Scholes European option price
        [QuantSAExcelFunction(Description = "Get the black scholes price of a European option.  Assumes lognormal dynamics.",
            Name = "QSA.BlackScholesPrice",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EuropeanOptionValuation.xlsx",
            IsHidden = false,
            HelpTopic = "")]
        public static string[,] BlackScholesPrice([QuantSAExcelArgument(Description = "The European Option.")] EuropeanOption option,
            [QuantSAExcelArgument(Description = "The valuation date of the option.")] Date valueDate,
            [QuantSAExcelArgument(Description = "The spot price of the underlying at the value date.")] double spot,
            [QuantSAExcelArgument(Description = "Annualized volatility.")] double vol,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] double rate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")] double div)
        {
            string[,] price = { { "Price", option.BlackScholesPrice(valueDate, spot, vol, rate, div).ToString() } };

            return price;
        }

        // Black-Scholes European option greeks
        [QuantSAExcelFunction(Description = "Get the black scholes greeks of a European option.  Assumes lognormal dynamics.",
            Name = "QSA.BlackScholesGreeks",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EuropeanOptionValuation.xlsx",
            IsHidden = false,
            HelpTopic = "")]
        public static string[,] BlackScholesGreeks([QuantSAExcelArgument(Description = "The European Option.")] EuropeanOption option,
            [QuantSAExcelArgument(Description = "The valuation date of the option.")] Date valueDate,
            [QuantSAExcelArgument(Description = "The spot price of the underlying at the value date.")] double spot,
            [QuantSAExcelArgument(Description = "Annualized volatility.")] double vol,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] double rate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")] double div)
        {
            var result = option.BlackScholesPrice(valueDate, spot, vol, rate, div);
            string[,] greeks = { { "Delta", option.BlackScholesPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Delta).ToString() },
            { "Gamma", option.BlackScholesPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Gamma).ToString() },
            { "Vega", option.BlackScholesPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Vega).ToString() },
            { "Theta", option.BlackScholesPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Theta).ToString() },
            { "Rho", option.BlackScholesPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Rho).ToString() }};

            return greeks;
        }

        // Monte Carlo European option price
        [QuantSAExcelFunction(Description = "Get the monte carlo price of a European option.  Assumes lognormal dynamics.",
            Name = "QSA.MonteCarloPrice",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EuropeanOptionValuation.xlsx",
            IsHidden = false,
            HelpTopic = "")]
        public static string[,] MonteCarloPrice([QuantSAExcelArgument(Description = "The European Option.")] EuropeanOption option,
            [QuantSAExcelArgument(Description = "The valuation date of the option.")] Date valueDate,
            [QuantSAExcelArgument(Description = "The spot price of the underlying at the value date.")] double spot,
            [QuantSAExcelArgument(Description = "Annualized volatility.")] double vol,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] double rate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")] double div,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] int numOfSims)
        {
            string[,] price = { { "Price", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Price, numOfSims).ToString() } };

            return price;
        }

        // Monte Carlo European option greeks
        [QuantSAExcelFunction(Description = "Get the monte carlo price of a European option.  Assumes lognormal dynamics.",
            Name = "QSA.MonteCarloGreeks",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EuropeanOptionValuation.xlsx",
            IsHidden = false,
            HelpTopic = "")]
        public static string[,] MonteCarloGreeks([QuantSAExcelArgument(Description = "The European Option.")] EuropeanOption option,
            [QuantSAExcelArgument(Description = "The valuation date of the option.")] Date valueDate,
            [QuantSAExcelArgument(Description = "The spot price of the underlying at the value date.")] double spot,
            [QuantSAExcelArgument(Description = "Annualized volatility.")] double vol,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] double rate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")] double div,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] int numOfSims)
        {
            string[,] greeks = { { "Delta", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Delta, numOfSims).ToString() },
            { "Gamma", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Gamma, numOfSims).ToString() },
            { "Vega", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Vega, numOfSims).ToString() },
            { "Theta", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Theta, numOfSims).ToString() },
            { "Rho", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, EuropeanOptionEx.OptionPriceandGreeks.Rho, numOfSims).ToString() }};

            return greeks;
        }


        
        // Create and Asian option object
        [QuantSAExcelFunction(Description = "Create an arithmetic asian option. Assumes lognormal dynamics.",
            Name = "QSA.CreateAsianOption",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "AsianOptionValuation.xlsx",
            IsHidden = false)]
        public static Product CreateAsianOption([QuantSAExcelArgument(Description = "A share.  This needs to match a share in the model that will be used to value this.")] Share share,
            [QuantSAExcelArgument(Description = "Exercise date.")] Date exerciseDate,
            [QuantSAExcelArgument(Description = "Strike")] double strike,
            [QuantSAExcelArgument(Description = "PutOrCall")] PutOrCall putOrCall)
        {
            return new AsianOption(share, putOrCall, strike, exerciseDate);
        }

        // Monte Carlo Asian option price
        [QuantSAExcelFunction(Description = "Get the price of an Arithmetic Asian option.  Assumes lognormal dynamics.",
            Name = "QSA.AsianOptionPrice",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EuropeanOptionValuation.xlsx",
            IsHidden = false,
            HelpTopic = "")]
        public static string[,] AsianOptionPrice([QuantSAExcelArgument(Description = "The European Option.")] AsianOption option,
            [QuantSAExcelArgument(Description = "The valuation date of the option.")] Date valueDate,
            [QuantSAExcelArgument(Description = "The spot price of the underlying at the value date.")] double spot,
            [QuantSAExcelArgument(Description = "Annualized volatility.")] double vol,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] double rate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")] double div,
            [QuantSAExcelArgument(Description = "The number of averaging periods.")] int periods)
        {
            string[,] price = { { "price", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, AsianOptionEx.OptionPriceandGreeks.Price, periods).ToString() } };

            return price;
        }

        // Monte Carlo Asian option greeks
        [QuantSAExcelFunction(Description = "Get the greeks of an Arithmetic Asian option.  Assumes lognormal dynamics.",
            Name = "QSA.AsianOptionGreeks",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "AsianOptionValuation.xlsx",
            IsHidden = false,
            HelpTopic = "")]
        public static string[,] AsianOptionGreeks([QuantSAExcelArgument(Description = "The European Option.")] AsianOption option,
            [QuantSAExcelArgument(Description = "The valuation date of the option.")] Date valueDate,
            [QuantSAExcelArgument(Description = "The spot price of the underlying at the value date.")] double spot,
            [QuantSAExcelArgument(Description = "Annualized volatility.")] double vol,
            [QuantSAExcelArgument(Description = "Continuously compounded risk free rate.")] double rate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")] double div,
            [QuantSAExcelArgument(Description = "The number of averaging periods.")] int periods)
        {
            string[,] results = { { "Delta", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, AsianOptionEx.OptionPriceandGreeks.Delta, periods).ToString() },
            { "Gamma", option.MonteCarloPriceAndGreeks(valueDate, spot, vol, rate, div, AsianOptionEx.OptionPriceandGreeks.Gamma, periods).ToString() }};

            return results;
        }


        
    }
}