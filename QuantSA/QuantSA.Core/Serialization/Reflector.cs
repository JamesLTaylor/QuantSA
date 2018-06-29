using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.Serialization
{
    public class Reflector
    {
        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            var assemblies = new[]
            {
                // TODO: Load via matching dlls when required.
                Assembly.GetAssembly(typeof(Date)), // QuantSA.Shared
                Assembly.GetAssembly(typeof(DatesAndRates)) // QuantSA.Core
            };
            return assemblies;
        }

        public static List<Type> GetAllConcreteImplementationsOf<T>()
        {
            var types = new List<Type>();
            foreach (var assembly in GetAllAssemblies())
                types.AddRange(assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Where(t => typeof(T).IsAssignableFrom(t)));

            return types;
        }
    }
}