using System;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using QuantSA.Shared.State;

namespace QuantSA.Excel.Addin.Config
{
    /// <summary>
    /// The <see cref="ILogFactory"/> used by the Excel add in.
    /// </summary>
    public class ExcelFileLogFactory : ILogFactory
    {
        private readonly FileAppender _appender;

        public ExcelFileLogFactory()
        {
            var now = DateTime.Now;
            var filename = now.ToString("yyyyddMM_HHmmss");
            _appender = new FileAppender();
            _appender.Name = "Excel Logger";
            _appender.File = $"C:/temp/QuantSA/QuantSA_{filename}.txt";
            _appender.AppendToFile = true;

            var layout = new PatternLayout();
            layout.ConversionPattern = "%d [%t] %-5p [%logger] - %m%n";
            layout.ActivateOptions();

            _appender.Layout = layout;
            _appender.ActivateOptions();
        }

        public ILog Get(Type type)
        {
            var log = LogManager.GetLogger(type);

            var l = (Logger)log.Logger;
            l.Level = l.Hierarchy.LevelMap["All"];
            l.AddAppender(_appender);
            l.Repository.Configured = true;

            return log;
        }
    }
}