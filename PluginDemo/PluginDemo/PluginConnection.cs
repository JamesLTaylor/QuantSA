using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Excel;

namespace PluginDemo
{
    public class PluginConnection : IQuantSAPlugin
    {

        public static ObjectMap objectMap;
        public static IQuantSAPlugin instance;

        public string GetDeveloper()
        {
            return "James Taylor";
        }

        public string GetName()
        {
            return "QuantSA demo plugin";
        }

        public void SetObjectMap(ObjectMap objectMap)
        {
            PluginConnection.objectMap = objectMap;
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

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Make sure that all button and group ids are unique.
        /// </remarks>
        /// <returns></returns>
        public string GetRibbonGroup()
        {
            return @"<group id='groupQSDEMO' label='Demo Plugin'>
            <button id='btnQSAShowAbout' label='About' imageMso='PropertySheet' size='large' onAction='RunTagMacro' tag='QSDEMO.ShowAbout' />         
            </group>";
        }

        public string GetAboutMacro()
        {
            return "QSDEMO.ShowAbout";
        }

        public void SetInstance(IQuantSAPlugin itself)
        {
            PluginConnection.instance = itself;
        }


    }
}
