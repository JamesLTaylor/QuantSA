using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Test.Dates
{
    [TestClass]
    public class TenorTests
    {
        [TestMethod]
        public void TenorFromString()
        {
            var tenor = new Tenor("1Y1M");
            Assert.AreEqual(new Tenor(0, 0, 1, 1), tenor);
        }
    }
}