using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Excel.Addin.Config;
using QuantSA.Shared.Serialization;

namespace QuantSA.Excel.Addin.Tests.Config
{
    [TestClass]
    public class StaticDataTests
    {
        [TestMethod]
        public void StaticDataTest()
        {
            StaticData.Load();
            var loaded = QuantSAState.SharedData;
        }
    }
}