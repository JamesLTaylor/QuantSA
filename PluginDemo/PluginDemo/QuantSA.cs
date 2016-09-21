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
            return "Demo plugin";
        }

        public void setObjectMap(ObjectMap objectMap)
        {
            QuantSA.objectMap = objectMap;
        }

        public ObjectMap getObjectMap()
        {
            return objectMap;
        }
    }
}
