using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.Rates;
using QuantSA.Excel.Addin.Functions;
using QuantSA.Shared.Dates;

namespace QuantSA.Excel.Addin.Tests.Functions
{
    [TestClass]
    public class ObjectViewerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var swap = IRSwap.CreateZARSwap(0.07, true, 100, new Date("2018-06-23"), Tenor.FromYears(2));
            var fields = ObjectViewer.ViewObjectPropertyNames(swap);

            var notionals = ObjectViewer.ViewObjectPropertyValue(swap, "notionals", null);

            var ccy = ObjectViewer.ViewObjectPropertyValue(swap, "ccy", "code");
        }
    }
}