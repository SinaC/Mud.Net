using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;

namespace Mud.Server.WPFTestApplication
{
    public class RichTextBoxSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public RichTextBoxSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            //"[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            logEvent.Properties.TryGetValue("SourceContext", out var sourceContext);
            var logEntry = $"[{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss} {FormatLevel(logEvent.Level)}] [{sourceContext}] {message}";
            if (logEvent.Exception != null)
            {
                logEntry += $"{Environment.NewLine}{logEvent.Exception}";
            }
            ServerWindow.SerilogMethod(logEvent.Level.ToString(), logEntry);
        }

        private string FormatLevel(LogEventLevel level)
            => level switch
            {
                LogEventLevel.Verbose => "VER",
                LogEventLevel.Debug => "DBG",
                LogEventLevel.Information => "INF",
                LogEventLevel.Warning => "WRN",
                LogEventLevel.Error => "ERR",
                LogEventLevel.Fatal => "FTL",
                _ => level.ToString()
            };
    }

    public static class RichTextBoxSinkExtensions
    {
        public static LoggerConfiguration RichTextBoxSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new RichTextBoxSink(formatProvider));
        }
    }
}
