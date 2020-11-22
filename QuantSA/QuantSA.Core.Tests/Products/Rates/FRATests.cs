using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;
using Assert = NUnit.Framework.Assert;

namespace QuantSA.Core.Tests.Products.Rates
{
    [TestClass]
    public class FRATests
    {
        [TestMethod]
        public void FRA_Clone()
        {
            var jibar = TestHelpers.Jibar3M;
            var zar = TestHelpers.ZAR;
            var fra = new FRA(1e6, 0.25, 0.07, true, new Date("2020-09-14"), new Date("2020-12-14"), jibar);
            var clone = fra.Clone();
            Assert.IsNotNull(clone);

            var indices = clone.GetRequiredIndices();
            Assert.AreEqual(jibar, indices[0]);
            Assert.That(indices, Has.Count.EqualTo(1));
            var cfCurrencies = clone.GetCashflowCurrencies();
            Assert.AreEqual(zar, cfCurrencies[0]);
            Assert.That(cfCurrencies, Has.Count.EqualTo(1));
        }
    }

}
