using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Solution.Test;

namespace QuantSA.Core.Tests.Primitives
{
    [TestClass]
    public class ProductTests
    {
        [TestMethod]
        public void ProductClone()
        {
            var swap = TestHelpers.ZARSwap();
            var clonedSwap = swap.Clone();
            Assert.IsNotNull(clonedSwap);
        }
    }
}
