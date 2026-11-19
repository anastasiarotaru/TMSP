using Delivery.Creational;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Services;
using Delivery.Structural.Adapter;
using Delivery.Structural.Decorator;
using Delivery.Structural.Facade;
using Delivery.Tests.Stubs;
using Serilog;

namespace Delivery.Tests;

// One execution path through all three patterns: Facade -> Decorator -> Adapter -> Serilog.
public class PatternInteractionTests
{
    [Fact]
    public void Facade_Decorator_Adapter_WorkTogetherOnOnePath()
    {
        var sink = new CollectingSink();
        ILogger logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        var history = new HistoryNotifier(new SerilogNotifierAdapter(logger));
        IShippingFacade facade = new ShippingFacade(
            new PricingFactory[] { new InternationalPricingFactory() },
            new DeliveryService(history),
            new StubLabelPrinter(new CallLog()));

        Parcel parcel = facade.Ship(new ShipmentRequest(
            "Ana", "ana@example.com", new Address("Str. 1", "Berlin"), 3.2, "international",
            new IPricingOption[] { new FragileOption() }));
        facade.DeliverToEnd(parcel.TrackingId);

        Assert.Equal(260.38, parcel.Price, 2);           // 250.38 international + 10 fragile
        Assert.Equal(ParcelStatus.Delivered, parcel.Status);
        Assert.Equal(4, history.History.Count);           // the decorator saw every notification
        Assert.Equal(4, sink.Events.Count);                // and passed each one to the adapter
    }
}
