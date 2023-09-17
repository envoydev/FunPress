using Serilog.Core;
using Serilog.Events;

namespace FunPress.Core.Logger.Enrichers
{
    internal class ClassNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue)
                || !(sourceContextValue is ScalarValue sourceContextScalar)
                || !(sourceContextScalar.Value is string sourceContext))
            {
                return;
            }

            var className = sourceContext.Substring(sourceContext.LastIndexOf('.') + 1);
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClassName", className));
        }
    }
}
