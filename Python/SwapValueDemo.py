import clr # pip install pythonnet

clr.AddReference("System.Collections")
clr.AddReference(r'C:\Dev\QuantSA\QuantSA\Valuation\bin\Debug\QuantSA.General.dll')
clr.AddReference(r'C:\Dev\QuantSA\QuantSA\Valuation\bin\Debug\QuantSA.Valuation.dll')

from System.Collections.Generic import List
from QuantSA.General import *
from QuantSA.Valuation import *

# Set up the swap
rate = 0.08
payFixed = True
notional = 1000000
startDate = Date(2016, 9, 17)
tenor = Tenor.Years(5)
swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor)


# Set up the model
valueDate = Date(2016, 9, 17)
maximumDate = Date(2026, 9, 17)
dates = [Date(2016, 9, 17) , Date(2026, 9, 17)]

rates = [ 0.07, 0.07 ]
discountCurve = DatesAndRates(Currency.ZAR, valueDate, dates, rates, maximumDate)

forecastCurve = ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates)
curveSim = DeterminsiticCurves(discountCurve);
curveSim.AddRateForecast(forecastCurve)

simulators = List[Simulator]()
coordinator = Coordinator(curveSim, simulators, 1)

# Run the valuation
portfolio = [swap]
value = coordinator.Value(portfolio, valueDate)
print("value is: {:.2f}.  Expected {:.2f}".format(value, -41838.32))
