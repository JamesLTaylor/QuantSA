using ExcelDna.Integration;
using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginDemo
{
    public class ExcelFunctions
    {       
        [QuantSAExcelFunction(Description = "",
            Name = "QSA.DEMO.CreateDiscount",
            Category = "QSA.DEMO",
            IsHidden = false)]
        public static object CreateDiscount([ExcelArgument(Description = "Name of object")]String name,
            [ExcelArgument(Description = "")]double anchorDate,
            [ExcelArgument(Description = "")]double rate)
        {
            try
            {
                PluginDiscount discount = new PluginDiscount(ExcelUtilities.GetDates(anchorDate), rate);                
                return QuantSA.objectMap.AddObject(name, discount);                
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }           
    }
}
