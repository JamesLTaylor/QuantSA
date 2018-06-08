using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Excel.Shared;

namespace QuantSA.Excel.Common
{
    public interface IQuantSAPlugin
    {
        string GetShortName();
        string GetName();
        string GetDeveloper();
        string GetAboutString();

        string GetRibbonGroup();
        string GetAboutMacro();

        void SetInstance(IQuantSAPlugin itself);
        void SetObjectMap(ObjectMap objectMap);
        ObjectMap getObjectMap();        
    }
}
