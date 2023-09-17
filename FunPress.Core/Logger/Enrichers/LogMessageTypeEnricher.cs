using Serilog.Core;
using Serilog.Events;

namespace FunPress.Core.Logger.Enrichers
{
    internal class LogMessageTypeEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var type = logEvent.Exception != null ? "EX" : "IN";

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LogMessageType", type));
        }
    }
}
