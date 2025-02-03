using Serilog.Events;
using Serilog.Formatting;

namespace Helicopters_Russia.Formatters
{
    public class CustomLokiJsonFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var logObject = new Dictionary<string, object>
            {
                {"Timestap", logEvent.Timestamp },
                {"Level", logEvent.Level },
                {"Message", logEvent.RenderMessage() }
            };

            foreach (var property in logEvent.Properties)
            {
                if (property.Key != "MessageTemplate")
                {
                    logObject[property.Key] = property.Value;
                }
            }
            output.WriteLine(System.Text.Json.JsonSerializer.Serialize(logObject));
        }
    }
}
