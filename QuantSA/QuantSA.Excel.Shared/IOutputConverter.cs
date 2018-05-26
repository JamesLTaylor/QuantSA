using System;

namespace QuantSA.Excel.Shared
{
    /// <summary>
    /// A class that converts an object coming from QuantSA or a plug in to a type that
    /// Excel can handle.
    /// </summary>
    public interface IOutputConverter
    {
        Type SuppliedType { get; }
        object Convert(object input);
    }
}
