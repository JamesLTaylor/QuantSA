using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Primitives.Conventions.Compounding;

namespace GeneralTest.Conventions.Compounding
{
    [TestClass]
    public class CompoundingTests
    {
        [TestMethod]
        public void SingleInstance()
        {
            CompoundingConvention annual1 = CompoundingStore.Annual;
            CompoundingConvention annual2 = CompoundingStore.Annual;
            Assert.IsTrue(annual1 == annual2);

            CompoundingConvention cont1 = CompoundingStore.Continuous;
            CompoundingConvention cont2 = CompoundingStore.Continuous;
            Assert.IsTrue(cont1 == cont2);
        }
        //TODO: Add compounding tests.
    }
}
