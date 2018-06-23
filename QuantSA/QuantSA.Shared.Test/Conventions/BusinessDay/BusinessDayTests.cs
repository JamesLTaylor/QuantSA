using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General.Dates;
using QuantSA.Shared.Conventions.BusinessDay;
using QuantSA.Shared.Dates;

namespace GeneralTest.Conventions.BusinessDay
{
    [TestClass]
    public class BusinessDayTests
    {
        [TestMethod]
        public void TestAdjustments()
        {
            var dateEndOfMonth = new Date(2016, 12, 31); // Saturday
            var calendar = new Calendar(new List<Date>()); // No holidays

            var testFollowing = BusinessDayStore.Following.Adjust(dateEndOfMonth, calendar);
            Assert.AreEqual(new Date(2017, 1, 2), testFollowing);

            var testModFollowing = BusinessDayStore.ModifiedFollowing.Adjust(dateEndOfMonth, calendar);
            Assert.AreEqual(new Date(2016, 12, 30), testModFollowing);

            var testNoAdjust = BusinessDayStore.Unadjusted.Adjust(dateEndOfMonth, calendar);
            Assert.AreEqual(new Date(2016, 12, 31), testNoAdjust);

            // Preceding
            var dateStartOfMonth = new Date(2017, 1, 1); // Sunday
            var testPreceding = BusinessDayStore.Preceding.Adjust(dateStartOfMonth, calendar);
            Assert.AreEqual(new Date(2016, 12, 30), testPreceding);

            var testModifiedPreceding = BusinessDayStore.ModifiedPreceding.Adjust(dateStartOfMonth, calendar);
            Assert.AreEqual(new Date(2017, 1, 2), testModifiedPreceding);
        }
    }
}