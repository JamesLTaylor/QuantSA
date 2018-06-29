using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}
