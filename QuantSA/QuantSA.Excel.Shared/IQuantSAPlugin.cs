namespace QuantSA.Excel.Shared
{
    public interface IQuantSAPlugin
    {
        string GetShortName();
        string GetName();
        string GetDeveloper();
        string GetAboutString();

        string GetRibbonGroup();
        string GetAboutMacro();
    }
}
