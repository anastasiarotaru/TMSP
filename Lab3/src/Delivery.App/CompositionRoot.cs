using Delivery.Behavioral.Command;
using Delivery.Behavioral.Observer;
using Delivery.Creational;
using Delivery.Services;
using Delivery.Structural.Adapter;
using Delivery.Structural.Decorator;
using Delivery.Structural.Facade;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.App;

// Everything the client needs, already connected.
public record DeliveryApp(
    IShippingFacade Shipping,
    IDeliveryService Delivery,
    IParcelEvents Events,
    CommandInvoker Commands,
    CustomerNotificationObserver Customer,
    AuditLogObserver Audit,
    StatusBoardObserver Board,
    HistoryNotifier History);

// The only place where concrete classes are chosen and connected.
public static class CompositionRoot
{
    public static DeliveryApp Create(SerilogLogger logger)
    {
        // Laboratory 2: Decorator (history) around the Adapter (Serilog).
        var history = new HistoryNotifier(new SerilogNotifierAdapter(logger));

        // Observer: one subject, three observers subscribed at start.
        var tracker = new ParcelTracker();
        var customer = new CustomerNotificationObserver(history);
        var audit = new AuditLogObserver();
        var board = new StatusBoardObserver();
        tracker.Subscribe(customer);
        tracker.Subscribe(audit);
        tracker.Subscribe(board);

        // Command receiver: publishes every change to the subject.
        IDeliveryService delivery = new DeliveryService(tracker);

        PricingFactory[] factories =
        {
            new StandardPricingFactory(),
            new ExpressPricingFactory(),
            new OvernightPricingFactory(),
            new InternationalPricingFactory()
        };

        IShippingFacade shipping = new ShippingFacade(factories, delivery, new LabelPrinter());

        return new DeliveryApp(shipping, delivery, tracker, new CommandInvoker(),
                               customer, audit, board, history);
    }
}
