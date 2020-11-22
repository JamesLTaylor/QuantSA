using System;
using QuantSA.CoreExtensions.Data;


namespace QuantSA.Excel.Addin.Config
{
    public static class StaticData
    {
        public static void Load()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            SharedDataLoader.LoadFromFolder(path);
        }
    }
}