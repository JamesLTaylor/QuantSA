using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Shared.Serialization
{
    public interface ISharedData
    {
        T Get<T>(string name) where T : ISerializableViaName;
        ISerializableViaName Get(Type type, string name);
    }
}
