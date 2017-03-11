using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.General.Conventions;
using QuantSA.General.Conventions.DayCount;

namespace GeneralTest.Conventions.DayCount
{
    [TestClass]
    public class DayCountTests
    {
        [TestMethod]
        public void TestDayCounts()
        {
            Date date1 = new Date(2003, 11, 1);
            Date date2 = new Date(2004, 5, 1);

            Assert.AreEqual(182.0 / 365, DayCountStore.Actual365Fixed.YearFraction(date1, date2), 1e-9);
                        
            Assert.AreEqual(61.0 / 365 + 121.0 / 366, DayCountStore.ActActISDA.YearFraction(date1, date2), 1e-9);
            Assert.AreEqual(0.497724380567, DayCountStore.ActActISDA.YearFraction(date1, date2), 1e-9);
        }
    }
}
