using System;
using System.Runtime.InteropServices;
using ExcelDna.Integration.CustomUI;
using System.Windows.Forms;
using ExcelDna.Integration;
using System.Text;
using QuantSA.Excel;
using QuantSA.Excel.Common;
using System.Drawing;

[ComVisible(true)]
public class Ribbon : ExcelRibbon
{
    public override string GetCustomUI(string uiName)
    {
        // One can find the standard available button iamges at http://soltechs.net/customui/imageMso01.asp?gal=1&count=no
   
        StringBuilder pluginSubmenu = new StringBuilder("");
        if (AddIn.Plugins.Count > 0)
        {
            pluginSubmenu.Append(@"<splitButton id='splitButton' size='large' >
                <button id = 'button' label = 'Plugins' />
                    <menu id = 'menu' >");
            foreach (IQuantSAPlugin plugin in AddIn.Plugins)
            {
                pluginSubmenu.Append("<button id = 'btn" + plugin.GetShortName() + "' label = '" + plugin.GetName() + "'" +
                    " onAction='RunTagMacro' tag='" + plugin.GetAboutMacro() + "'/>");
            }

            pluginSubmenu.Append(@"</menu>
            </splitButton>");
        }

        string commonGroup = @"<group id='groupCommon' label='QuantSA'>
            <button id='btnAbout' label='About' image='LogoTemp1_256' size='large' onAction='RunTagMacro' tag='QSA.ShowAbout' />           
            <button id='btnOpenExcel' label='Example Sheets' imageMso='FileOpen' size='large' onAction='RunTagMacro' tag='QSA.OpenExampleSheetsDir' />
            <button id='btnLatestError' label='Latest Error' imageMso='Risks' size='large' onAction='RunTagMacro' tag='QSA.LatestError' />"
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

        foreach (IQuantSAPlugin plugin in AddIn.Plugins)
        {
            customUI += plugin.GetRibbonGroup();
        }
        customUI += customUIEnd; 
        return customUI;
    }

    /// <summary>
    /// Used when an image tag is found in the ribbon xml.  First checks in the image resources from the plugins, 
    /// then in the resources of this assembly then trys to use a standard imageMso.
    /// </summary>
    /// <param name="imageId">The image identifier.</param>
    /// <returns></returns>
    public override object LoadImage(string imageId)
    {
        if (AddIn.AssemblyImageResources.ContainsKey(imageId))
            return (AddIn.AssemblyImageResources[imageId]);

        try
        {
            return QuantSA.Excel.Addin.Properties.Resources.ResourceManager.GetObject(imageId);
        }
        catch { }
        
        return base.LoadImage(imageId);
    }

    [ExcelFunction(IsMacroType = true, Description = "Show information about QuantSA", Name = "QSA.ShowAbout", Category = "QSA.General", IsHidden = true)]
    public static int ShowAbout()
    {
        ExcelMessage em = new ExcelMessage("QuantSA", "QuantSA is an open source library for quantitative finance, customized for the South African market\n\n" +
            "View the code at: https://github.com/JamesLTaylor/QuantSA \n\n" +
            "Visit the website at www.quantsa.org");
        em.ShowDialog();
        return 0;
    }
}
