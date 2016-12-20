Date date1 = new Date(2016, 9, 30); // The issue date of the scheme
Date date2 = new Date(2017, 9, 30); // The first performance measurment date
Date date3 = new Date(2018, 9, 30); // The second performance measurement date and the payment date.
Share aaa = new Share("AAA", Currency.ZAR);
Share alsi = new Share("ALSI", Currency.ZAR);
double threshAbs = 0.10; // AAA share must return at least 10% each year 
double threshRel = 0.03; // AA share must outperform the ALSI by at least 3% in each year

public override List<Cashflow> GetCFs()
{
    double w1; double w2;
    double year1AAAReturn = Get(aaa, date2) / Get(aaa, date1) - 1;
    double year2AAAReturn = Get(aaa, date3) / Get(aaa, date2) - 1;
    double year1ALSIReturn = Get(alsi, date2) / Get(alsi, date1) - 1;
    double year2ALSIReturn = Get(alsi, date3) / Get(alsi, date2) - 1;
    if ((year1AAAReturn > threshAbs) && year2AAAReturn > threshAbs)
        w1 = 1.0;
    else
        w1 = 0.0;

    if ((year1AAAReturn - year1ALSIReturn) > threshRel && (year2AAAReturn - year2ALSIReturn) > threshRel)
        w2 = 1.0;
    else
        w2 = 0.0;

    return new List<Cashflow> { new Cashflow(date3, Get(aaa, date3) * (w1 + w2), Currency.ZAR) };
}
