using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.General.Conventions.BusinessDay;
using QuantSA.General.Dates;
using System.Collections.Generic;

namespace GeneralTest.Conventions.BusinessDay
{
    [TestClass]
    public class BusinessDayTests
    {
        [TestMethod]
        public void TestAdjustments()
        {
            Date dateEndOfMonth = new Date(2016, 12, 31); // Saturday
            Calendar calendar = new Calendar(new List<Date>()); // No holidays

            Date testFollowing = BusinessDayStore.Following.Adjust(dateEndOfMonth, calendar);
            Assert.AreEqual(new Date(2017, 1, 2), testFollowing);

            Date testModFollowing = BusinessDayStore.ModifiedFollowing.Adjust(dateEndOfMonth, calendar);
            Assert.AreEqual(new Date(2016, 12, 30), testModFollowing);

            Date testNoAdjust = BusinessDayStore.Unadjusted.Adjust(dateEndOfMonth, calendar);
            Assert.AreEqual(new Date(2016, 12, 31), testNoAdjust);

            // Preceding
            Date dateStartOfMonth = new Date(2017, 1, 1); // Sunday
            Date testPreceding = BusinessDayStore.Preceding.Adjust(dateStartOfMonth, calendar);
            Assert.AreEqual(new Date(2016, 12, 30), testPreceding);

            Date testModifiedPreceding = BusinessDayStore.ModifiedPreceding.Adjust(dateStartOfMonth, calendar);
            Assert.AreEqual(new Date(2017, 1, 2), testModifiedPreceding);
        }
    }
}
