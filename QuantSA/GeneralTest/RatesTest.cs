using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.General.Dates;

namespace GeneralTest
{
    [TestClass]
    public class RatesTest
    {
        [TestMethod]
        public void TestZARSwapCurveStripper()
        {
            Date anchorDate = new Date(2016, 08, 30);
            double jibar = 0.075;
            string[] fraDescriptions = new string[] { "1x4", "2x5", "3x6", "4x7", "5x8", "6x9", "7x10", "8x11", "9x12", "12x15", "15x18", "18x21", "21x24" };
            double[] fraRates = new double[] { 0.0750, 0.0752, 0.0759, 0.0761, 0.0768, 0.0770, 0.0775, 0.0776, 0.0780, 0.0781, 0.0783, 0.0784, 0.0785 };
            int[] swapTenors = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20 };
            double[] swapRates = new double[] { 0.0784, 0.0797, 0.0809, 0.0819, 0.0829, 0.0836, 0.0842, 0.0846, 0.0852, 0.0853, 0.0845 };

            Stripper.ZARSwapCurve(jibar, fraDescriptions, fraRates, swapTenors, swapRates);
        }
    }
}
