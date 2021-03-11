using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Test.Dates
{
    [TestClass]
    public class CalendarTests
    {
        [TestMethod]
        public void Calendar_CanLoad()
        {
            var calendar = new Calendar("Test");

            Assert.IsNotNull(calendar);
        }
    }
}
