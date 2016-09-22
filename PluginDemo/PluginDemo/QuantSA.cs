using Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Excel;

namespace PluginDemo
{
    public class QuantSA : IQuantSAPlugin
    {

        public static ObjectMap objectMap;

        public string GetDeveloper()
        {
            return "James Taylor";
        }

        public string GetName()
        {
            return "QuantSA demo plugin";
        }

        public void setObjectMap(ObjectMap objectMap)
        {
            QuantSA.objectMap = objectMap;
        }

        public ObjectMap getObjectMap()
        {
            return objectMap;
        }

        public string GetShortName()
        {
            return "QSDEMO";
        }

        public string GetAboutString()
        {
            return "The demo plugin shows developers how to write a QuantSA plugin.\n\n"+
                "It defines an object that implements a QuantSA interface and it uses an object that is defined in QuantSA.";
        }
    }
}
