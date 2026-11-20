using Delivery.Notifications;
using Serilog.Events;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.Structural.Adapter;

// ADAPTER
//   Target  = INotifier (our interface: Send(recipient, message))
//   Adaptee = Serilog.ILogger (third-party library we cannot change:
//             Write(level, messageTemplate, propertyValues), ForContext(name, value))
//
// The adapter lets the delivery system send customer notifications into the
// company's Serilog pipeline without DeliveryService knowing about Serilog.
public class SerilogNotifierAdapter : INotifier
{
    public const string MessageTemplate = "Notification to {Recipient:l}: {Text:l}";
    public const string ChannelName = "customer-notification";

    private readonly SerilogLogger logger;

    public SerilogNotifierAdapter(SerilogLogger logger)
    {
        // Every event written through the adapter is tagged with the channel it came from.
        this.logger = logger.ForContext("Channel", ChannelName);
    }

    public void Send(string recipient, string message)
    {
        // Translation: a notification without a recipient is not an error for us,
        // but in the logging world it must be visible, so it becomes a Warning.
        LogEventLevel level = string.IsNullOrWhiteSpace(recipient)
            ? LogEventLevel.Warning
            : LogEventLevel.Information;

        // Translation: the two plain strings become named, structured properties.
        logger.Write(level, MessageTemplate, recipient, message);
    }
}
