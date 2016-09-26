path = fileparts(which(mfilename()));
NET.addAssembly([path,'\..\QuantSA\Excel\bin\Debug\QuantSA.General.dll'])
NET.addAssembly([path,'\..\QuantSA\Excel\bin\Debug\QuantSA.MonteCarlo.dll'])

% Set up the swap
rate = 0.08;
payFixed = true;
notional = 1000000;
startDate = QuantSA.Date(2016, 9, 17);
tenor = QuantSA.Tenor.Years(5);
swap = QuantSA.IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

% Set up the model
valueDate = QuantSA.Date(2016, 9, 17);
maximumDate = QuantSA.Date(2026, 9, 17);
dates = NET.createArray('QuantSA.Date',2);
dates.Set(0, QuantSA.Date(2016, 9, 17));
dates.Set(1, QuantSA.Date(2026, 9, 17));

rates = [ 0.07, 0.07 ];
discountCurve = QuantSA.DatesAndRates(QuantSA.Currency.ZAR, ...
    valueDate, dates, rates, maximumDate);

forecastCurve = QuantSA.ForecastCurve(valueDate, QuantSA.FloatingIndex.JIBAR3M, dates, rates);
curveSim = QuantSA.DeterminsiticCurves(discountCurve);
curveSim.AddRateForecast(forecastCurve);
simulators = NET.createGeneric('System.Collections.Generic.List',{'MonteCarlo.Simulator'},0);
coordinator = MonteCarlo.Coordinator(curveSim, simulators, 1);

% Run the valuation
portfolio = NET.createGeneric('System.Collections.Generic.List',{'MonteCarlo.Product'},1);
portfolio.Add(swap)
value = coordinator.Value(portfolio, valueDate);
sprintf('value is: %0.2f.  Expected %0.2f',value, -41838.32)
