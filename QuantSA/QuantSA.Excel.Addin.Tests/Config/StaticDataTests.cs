using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Excel.Addin.Config;
using QuantSA.Shared.Serialization;

namespace QuantSA.Excel.Addin.Tests.Config
{
    [TestClass]
    public class StaticDataTests
    {
        /// <summary>
        /// Tests that all the static data loads without throwing an error.
        /// </summary>
        [TestMethod]
        public void StaticDataTest()
        {
            StaticData.Load();
            var loaded = QuantSAState.SharedData;
        }
    }
}