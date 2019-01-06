using System;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using QuantSA.Shared.State;

namespace QuantSA.Solution.Test
{
    public class TestLogger : ILogFactory
    {
        private readonly TraceAppender _appender;

        public TestLogger()
        {
            _appender = new TraceAppender();
            _appender.Name = "Test Logger";

            var layout = new PatternLayout();
            layout.ConversionPattern = "%d [%t] %-5p [%logger] - %m%n";
            layout.ActivateOptions();

            _appender.Layout = layout;
            _appender.ActivateOptions();
        }

        public ILog Get(Type type)
        {
            var log = LogManager.GetLogger(type);

            var l = (Logger) log.Logger;
            l.Level = l.Hierarchy.LevelMap["All"];
            l.AddAppender(_appender);
            l.Repository.Configured = true;

            return log;
        }
    }
}