using System;
using System.Runtime.InteropServices;
using ExcelDna.Integration.CustomUI;
using System.Windows.Forms;
using ExcelDna.Integration;
using System.Text;
using QuantSA.Excel;
using System.Drawing;

[ComVisible(true)]
public class Ribbon : ExcelRibbon
{
    public override string GetCustomUI(string uiName)
    {
        // One can find the standard available button iamges at http://soltechs.net/customui/imageMso01.asp?gal=1&count=no
   
        StringBuilder pluginSubmenu = new StringBuilder("");
        if (MyAddIn.plugins.Count > 0)
        {
            pluginSubmenu.Append(@"<splitButton id='splitButton' size='large' >
                <button id = 'button' label = 'Plugins' />
                    <menu id = 'menu' >");
            foreach (IQuantSAPlugin plugin in MyAddIn.plugins)
            {
                pluginSubmenu.Append("<button id = 'btn" + plugin.GetShortName() + "' label = '" + plugin.GetName() + "'" +
                    " onAction='RunTagMacro' tag='" + plugin.GetAboutMacro() + "'/>");
            }

            pluginSubmenu.Append(@"</menu>
            </splitButton>");
        }

        string commonGroup = @"<group id='groupCommon' label='QuantSA'>
            <button id='btnAbout' label='About' imageMso='PropertySheet' size='large' onAction='RunTagMacro' tag='QSA.ShowAbout' />            
            <button id='btnLatestError' label='Latest Error' size='large' onAction='RunTagMacro' tag='QSA.LatestError' />"
            + pluginSubmenu.ToString()
            + "</group>";
        string customUIStart = @"<customUI xmlns='http://schemas.microsoft.com/office/2006/01/customui' loadImage='LoadImage'>
                <ribbon>
                <tabs>
                <tab id='tabQuantSA' label='QuantSA'>";

        string customUIEnd = @"</tab>
                </tabs>
                </ribbon>
                </customUI>";

        string customUI = customUIStart;
        customUI += commonGroup;

        foreach (IQuantSAPlugin plugin in MyAddIn.plugins)
        {
            customUI += plugin.GetRibbonGroup();
        }
        customUI += customUIEnd; 
        return customUI;
    }

    public override object LoadImage(string imageId)
    {
        if (MyAddIn.assemblyImageResources.ContainsKey(imageId))
            return (MyAddIn.assemblyImageResources[imageId]);

        return base.LoadImage(imageId);
    }

    [ExcelFunction(IsMacroType = true, Description = "Show information about QuantSA", Name = "QSA.ShowAbout", Category = "QSA.General", IsHidden = true)]
    public static int ShowAbout()
    {
        ExcelMessage em = new ExcelMessage("QuantSA", "QuantSA is an open source Quant Library customized for the South African Market\n\n" +
            "View the code at: https://github.com/JamesLTaylor/QuantSA \n\n" +
            "Visit the website at www.quantsa.org");
        em.ShowDialog();
        return 0;
    }
}
