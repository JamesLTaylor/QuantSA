using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.ProductExtensions.SAMarket;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace QuantSA.Solution.Test
{
    [TestClass]
    public class SerializationTests
    {
        private static IEnumerable<Assembly> GetAllAssemblies()
        {
            var assemblies = new[]
            {
                Assembly.GetAssembly(typeof(Date)), // QuantSA.Shared
                Assembly.GetAssembly(typeof(DatesAndRates)), // QuantSA.Core
                Assembly.GetAssembly(typeof(Coordinator)), // QuantSA.Valuation
                Assembly.GetAssembly(typeof(BesaJseBondEx)) // QuantSA.CoreExtensions
            };
            return assemblies;
        }

        private List<Type> GetAllConcreteImplementationsOf<T>()
        {
            var types = new List<Type>();
            foreach (var assembly in GetAllAssemblies())
                types.AddRange(assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Where(t => typeof(T).IsAssignableFrom(t)));

            return types;
        }

        [TestMethod]
        public void AllProducts_AreSerializable()
        {
            foreach (var type in GetAllConcreteImplementationsOf<IProduct>())
            {
                if (type.GetCustomAttribute<JsonObjectAttribute>() != null) continue;
                Assert.Fail($"{type.FullName} is not Json serializable");
            }
        }
    }
}