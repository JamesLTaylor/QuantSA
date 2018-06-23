using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.Rates;
using QuantSA.Excel.Addin.Functions;
using QuantSA.Shared.Dates;

namespace QuantSA.Excel.Addin.Tests.Functions
{
    [TestClass]
    public class ObjectViewerTests
    {
        private IRSwap _swap;

        [TestInitialize]
        public void SetUp()
        {
            _swap = IRSwap.CreateZARSwap(0.07, true, 100, new Date("2018-06-23"), Tenor.FromYears(2));
        }

        [TestMethod]
        public void ObjectViewer_ViewObjectPropertyNames_GetAllNonIgnore()
        {
            var fields = ObjectViewer.ViewObjectPropertyNames(_swap);
            Assert.AreEqual(9, fields.Length);
        }

        [TestMethod]
        public void ObjectViewer_ViewObjectPropertyValue_OneLevel()
        {
            var notionals = ObjectViewer.ViewObjectPropertyValue(_swap, "notionals", null);
            Assert.AreEqual(8, notionals.GetLength(0));
        }

        [TestMethod]
        public void ObjectViewer_ViewObjectPropertyValue_TwoLevel()
        {
            var ccy = ObjectViewer.ViewObjectPropertyValue(_swap, "ccy", "code");
            Assert.AreEqual("ZAR", (string) ccy[0, 0]);
        }
    }
}