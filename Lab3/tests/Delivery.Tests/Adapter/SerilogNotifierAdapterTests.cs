using Delivery.Behavioral.Observer;
using Delivery.Domain;
using Delivery.Notifications;
using Delivery.Pricing;
using Delivery.Services;
using Delivery.Structural.Adapter;
using Delivery.Tests.Stubs;
using Serilog;
using Serilog.Events;

namespace Delivery.Tests.Adapter;

public class SerilogNotifierAdapterTests
{
    private readonly CollectingSink sink = new();
    private readonly INotifier notifier;

    public SerilogNotifierAdapterTests()
    {
        ILogger logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(sink).CreateLogger();
        notifier = new SerilogNotifierAdapter(logger);
    }

    private static object? Property(LogEvent e, string name) => ((ScalarValue)e.Properties[name]).Value;

    [Fact]
    public void Send_TranslatesTheCallIntoOneStructuredSerilogEvent()
    {
        notifier.Send("ana@example.com", "Parcel RO1 registered");

        LogEvent e = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Information, e.Level);
        Assert.Equal(SerilogNotifierAdapter.MessageTemplate, e.MessageTemplate.Text);
        Assert.Equal("ana@example.com", Property(e, "Recipient"));
        Assert.Equal("Parcel RO1 registered", Property(e, "Text"));
        Assert.Equal("Notification to ana@example.com: Parcel RO1 registered", e.RenderMessage());
    }

    [Fact]
    public void Send_TagsEveryEventWithTheNotificationChannel()
    {
        notifier.Send("ana@example.com", "first");
        notifier.Send("ion@example.com", "second");

        Assert.All(sink.Events, e =>
            Assert.Equal(SerilogNotifierAdapter.ChannelName, Property(e, "Channel")));
    }

    [Fact]
    public void Send_WithoutRecipient_IsTranslatedIntoAWarning()
    {
        notifier.Send("", "nobody will read this");

        LogEvent e = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Warning, e.Level);
    }

    [Fact]
    public void DeliveryService_ReachesTheAdapterOnlyThroughAnObserverAndINotifier()
    {
        var tracker = new ParcelTracker();
        tracker.Subscribe(new CustomerNotificationObserver(notifier));
        var delivery = new DeliveryService(tracker);

        Parcel parcel = delivery.Register("Ana", "ana@example.com", new Address("Str. 1", "Chisinau"),
                                          2, "standard", new StandardPricing());

        LogEvent e = Assert.Single(sink.Events);
        Assert.Contains(parcel.TrackingId, (string?)Property(e, "Text"));
    }
}
