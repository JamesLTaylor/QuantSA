using System.Runtime.InteropServices;
using System.Text;
using ExcelDna.Integration.CustomUI;
using QuantSA.Excel.Addin;
using QuantSA.Excel.Addin.Properties;
using QuantSA.Excel.Shared;

[ComVisible(true)]
// ReSharper disable once CheckNamespace
// Namespace matches ExcelDna
public class Ribbon : ExcelRibbon
{
    public override string GetCustomUI(string uiName)
    {
        // One can find the standard available button images at http://soltechs.net/customui/imageMso01.asp?gal=1&count=no

        var pluginSubmenu = new StringBuilder("");
        if (AddIn.Plugins.Count > 0)
        {
            pluginSubmenu.Append(@"<splitButton id='splitButton' size='large' >
                <button id = 'button' label = 'Plugins' />
                    <menu id = 'menu' >");
            foreach (var plugin in AddIn.Plugins)
                pluginSubmenu.Append($"<button id = 'btn{plugin.Item1.GetShortName()}' label = " +
                                     $"'{plugin.Item1.GetName()}' onAction='RunTagMacro' " +
                                     $"tag='{plugin.Item1.GetAboutMacro()}'/>");

            pluginSubmenu.Append(@"</menu>
            </splitButton>");
        }

        var commonGroup = @"<group id='groupCommon' label='QuantSA'>
            <button id='btnAbout' label='About' image='LogoTemp1_256' size='large' onAction='RunTagMacro' tag='QSA.ShowAbout' />           
            <button id='btnOpenExcel' label='Example Sheets' imageMso='FileOpen' size='large' onAction='RunTagMacro' tag='QSA.OpenExampleSheetsDir' />
            <button id='btnLatestError' label='Latest Error' imageMso='Risks' size='large' onAction='RunTagMacro' tag='QSA.LatestError' />"
                          + pluginSubmenu
                          + "</group>";
        var customUIStart =
            @"<customUI xmlns='http://schemas.microsoft.com/office/2006/01/customui' loadImage='LoadImage'>
                <ribbon>
                <tabs>
                <tab id='tabQuantSA' label='QuantSA'>";

        var customUIEnd = @"</tab>
                </tabs>
                </ribbon>
                </customUI>";

        var customUI = customUIStart;
        customUI += commonGroup;

        foreach (var plugin in AddIn.Plugins) customUI += plugin.Item1.GetRibbonGroup();
        customUI += customUIEnd;
        return customUI;
    }

    /// <summary>
    /// Used when an image tag is found in the ribbon xml.  First checks in the image resources from the plugins, 
    /// then in the resources of this assembly then tries to use a standard imageMso.
    /// </summary>
    /// <param name="imageId">The image identifier.</param>
    /// <returns></returns>
    public override object LoadImage(string imageId)
    {
        if (AddIn.AssemblyImageResources.ContainsKey(imageId))
            return AddIn.AssemblyImageResources[imageId];

        try
        {
            return Resources.ResourceManager.GetObject(imageId);
        }
        catch
        {
        }

        return base.LoadImage(imageId);
    }

    [QuantSAExcelFunction(IsMacroType = true, Description = "Show information about QuantSA", Name = "QSA.ShowAbout",
        Category = "QSA.General", IsHidden = true)]
    public static int ShowAbout()
    {
        var em = new ExcelMessage("QuantSA",
            "QuantSA is an open source library for quantitative finance, customized for the South African market\n\n" +
            "View the code at: https://github.com/JamesLTaylor/QuantSA \n\n" +
            "Visit the website at www.quantsa.org");
        em.ShowDialog();
        return 0;
    }
}