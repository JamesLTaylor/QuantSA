using System;
using log4net;

namespace QuantSA.Shared.State
{
    /// <summary>
    /// Creates an <see cref="ILog"/> instance.
    /// </summary>
    public interface ILogFactory
    {
        ILog Get(Type type);
    }
}
