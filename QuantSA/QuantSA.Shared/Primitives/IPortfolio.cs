using System.Collections.Generic;

namespace QuantSA.Shared.Primitives
{
    /// <summary>
    /// A collection of products.
    /// </summary>
    public interface IPortfolio
    {
        IEnumerable<IProduct> Products { get; }
    }
}