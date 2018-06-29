using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products.Rates;
using QuantSA.Core.Serialization;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Rates;

namespace QuantSA.Solution.Test
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void DatesAndRates_CanCloneViaSerialize()
        {
            var curve = TestHelpers.FlatDiscountCurve();
            var newCurve = (DatesAndRates) Cloner.Clone(curve);
            var testDate = curve.GetAnchorDate().AddMonths(12);
            Assert.AreEqual(curve.GetDF(testDate), newCurve.GetDF(testDate), 1e-8);
        }

        [TestMethod]
        public void IRSwap_CanCloneViaSerialize()
        {
            var swap = TestHelpers.ZARSwap();
            var newSwap = (IRSwap) Cloner.Clone(swap);
        }

        [TestMethod]
        public void HullWhite1F_CanCloneViaSerialize()
        {
            var model = new HullWhite1F(TestHelpers.ZAR, 0.05, 0.01, 0.07, 0.07);
            var newModel = (HullWhite1F) Cloner.Clone(model);
        }

        /*
        [TestMethod]
        public void AllProducts_AreSerializable()
        {
            foreach (var type in Reflector.GetAllConcreteImplementationsOf<IProduct>())
            {
                if (type.GetCustomAttribute<JsonObjectAttribute>() != null) continue;
                Assert.Fail($"{type.FullName} is not Json serializable");
            }
        }
        */
    }
}