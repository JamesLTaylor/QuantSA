using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General.Dates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralTest.Dates
{
    [TestClass]
    public class CalendarTest
    {
        [TestMethod]
        public void TestCalendarFromFile()
        {
            Calendar calendar = Calendar.FromFile("./TestData/TestCalendar.csv");
            
        }

    }
}
