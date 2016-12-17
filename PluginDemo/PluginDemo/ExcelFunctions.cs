using ExcelDna.Integration;
using QuantSA.General;
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
        Name = "QSDEMO.ShowAbout",
        Category = "QSDEMO",
            IsMacroType = true,
        IsHidden = true)]
        public static object ShowAbout()
        {
            ExcelMessage message = new ExcelMessage("QuantSA Plugin demo.", PluginConnection.instance.GetAboutString());
            message.ShowDialog();
            return null;
        }

        [QuantSAExcelFunction(Description = "",
            Name = "QSDEMO.CreateDiscount",
            Category = "QSDEMO",
            IsHidden = false)]
        public static object CreateDiscount([ExcelArgument(Description = "Name of object")]String name,
            [ExcelArgument(Description = "")]object[,] anchorDate,
            [ExcelArgument(Description = "")]double rate)
        {
            try
            {
                PluginDiscount discount = new PluginDiscount(ExcelUtilities.GetDate0D(anchorDate, "anchorDate"), rate);                
                return PluginConnection.objectMap.AddObject(name, discount);                
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "",
        Name = "QSDEMO.GetSpecialDF",
        Category = "QSDEMO",
        IsHidden = false)]
        public static object GetSpecialDF([ExcelArgument(Description = "Name of discounting curve.")]String name,
        [ExcelArgument(Description = "date")]object[,] date)
        {
            try
            {
                IDiscountingSource discountCurve = PluginConnection.objectMap.GetObjectFromID<IDiscountingSource>(name);
                return discountCurve.GetDF(ExcelUtilities.GetDate0D(date, "date"));
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);                
            }
        }
    }
}
