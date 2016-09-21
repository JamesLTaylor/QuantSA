using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excel
{
    public interface IQuantSAPlugin
    {
        string GetName();
        string GetDeveloper();
        void setObjectMap(ObjectMap objectMap);
        ObjectMap getObjectMap();      
    }
}
