using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Conventions.Compounding;

namespace GeneralTest.Conventions.DayCount
{
    [TestClass]
    public class CompoundingTests
    {
        [TestMethod]
        public void SingleInstance()
        {
            ICompoundingConvention annual1 = CompoundingStore.Annual;
            ICompoundingConvention annual2 = CompoundingStore.Annual;
            Assert.IsTrue(annual1 == annual2);

            ICompoundingConvention cont1 = CompoundingStore.Continuous;
            ICompoundingConvention cont2 = CompoundingStore.Continuous;
            Assert.IsTrue(cont1 == cont2);
        }
        //TODO: Add compounding tests.
    }
}
