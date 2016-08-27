using System;
using System.Runtime.InteropServices;
using ExcelDna.Integration.CustomUI;
using System.Windows.Forms;
using ExcelDna.Integration;

[ComVisible(true)]
public class Ribbon : ExcelRibbon
{
    public override string GetCustomUI(string uiName)
    {
        string commonGroup = @"<group id='groupCommon' label='QuantSA'>
            <button id='btnAbout' label='About' imageMso='PropertySheet' size='large' onAction='RunTagMacro' tag='QSA.ShowAbout' />
            </group >";
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
        customUI += customUIEnd;

        return customUI;
    }

    [ExcelFunction(IsMacroType = true, Description = "Show information about QuantSA", Name = "QSA.ShowAbout", Category = "QSA.General", IsHidden = true)]
    public static int ShowAbout()
    {
        MessageBox.Show("QuantSA is an open source Quant Library customized for the South African Market\n\n" +
            "View the code at: https://github.com/JamesLTaylor/Edenglen \n\n" + 
            "Visit the website at www.cogn.co.za/QuantSA", "QuantSA", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }
}
