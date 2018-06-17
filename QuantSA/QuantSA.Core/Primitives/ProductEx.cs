using System.Collections.Generic;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Primitives
{
    public static class ProductExtensions
    {
        /// <summary>
        /// Makes a deep copy of the List of <see cref="IProduct"/>s
        /// </summary>
        /// <param name="originalPortfolio">The original portfolio.</param>
        /// <returns></returns>
        public static List<IProduct> Clone(this List<IProduct> originalPortfolio)
        {
            var newPortfolio = new List<IProduct>();
            foreach (var p in originalPortfolio) newPortfolio.Add(p.Clone());
            return newPortfolio;
        }
    }
}