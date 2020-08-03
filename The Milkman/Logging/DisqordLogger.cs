using Disqord.Logging;
using Microsoft.Extensions.Logging;
using System;
using IMSLogger = Microsoft.Extensions.Logging.ILogger;
using IDisqordLogger = Disqord.Logging.ILogger;

namespace The_Milkman.Logging
{
    public class DisqordLogger : IDisqordLogger
    {
        private readonly IMSLogger _logger;

        public DisqordLogger(IMSLogger logger)
            => _logger = logger;

        public void Log(object sender, LogEventArgs e)
            => _logger.Log((LogLevel)(int)e.Severity, e.Exception, "[{source}] : {message}", e.Source, e.Message);

        public event EventHandler<LogEventArgs> Logged;
        public void Dispose() { }
    }
}
