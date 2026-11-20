using Delivery.Behavioral.Command;
using Delivery.Behavioral.Observer;
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

// One execution path through the patterns:
// Command -> Receiver -> State -> Subject -> Observers -> Decorator -> Adapter -> Serilog.
public class PatternInteractionTests
{
    [Fact]
    public void Command_ChangesState_AndEveryObserverIsNotified_ThroughTheLab2Chain()
    {
        var sink = new CollectingSink();
        ILogger logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        var history = new HistoryNotifier(new SerilogNotifierAdapter(logger));

        var tracker = new ParcelTracker();
        var board = new StatusBoardObserver();
        tracker.Subscribe(new CustomerNotificationObserver(history));
        tracker.Subscribe(board);

        IDeliveryService delivery = new DeliveryService(tracker);
        IShippingFacade facade = new ShippingFacade(
            new PricingFactory[] { new InternationalPricingFactory() }, delivery, new StubLabelPrinter(new CallLog()));
        var invoker = new CommandInvoker();

        Parcel parcel = facade.Ship(new ShipmentRequest(
            "Ana", "ana@example.com", new Address("Str. 1", "Berlin"), 3.2, "international",
            new IPricingOption[] { new FragileOption() }));
        invoker.Execute(new AdvanceParcelCommand(delivery, parcel.TrackingId));

        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);   // State changed by the Command
        Assert.Equal(1, board.CountIn(ParcelStatus.PickedUp)); // Observer 1
        Assert.Equal(2, history.History.Count);                // Observer 2 -> Decorator
        Assert.Equal(2, sink.Events.Count);                    // -> Adapter -> Serilog
        Assert.Contains("PickedUp", sink.Events[^1].RenderMessage());
    }
}
