using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General.Conventions.Compounding;

namespace GeneralTest.Conventions.DayCount
{
    [TestClass]
    public class CompoundingTests
    {
        [TestMethod]
        public void SingleInstance()
        {
            ICompounding annual1 = CompoundingStore.Annual;
            ICompounding annual2 = CompoundingStore.Annual;
            Assert.IsTrue(annual1 == annual2);

            ICompounding cont1 = CompoundingStore.Continuous;
            ICompounding cont2 = CompoundingStore.Continuous;
            Assert.IsTrue(cont1 == cont2);
        }
        //TODO: Add compounding tests.
    }
}
