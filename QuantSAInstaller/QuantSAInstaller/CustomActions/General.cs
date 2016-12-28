using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSAInstaller
{
    enum Bitness
    {
        Bitness32,
        Bitness64
    }

    class BitInstance
    {
        public BitInstance(string data, Bitness bitness)
        {
            Data = data;
            Bitness = Bitness;
        }

        public string Data { get; private set; }
        public Bitness Bitness { get; private set; }
    }
}
