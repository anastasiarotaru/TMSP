using Delivery.Creational;
using Delivery.Services;
using Delivery.Structural.Adapter;
using Delivery.Structural.Decorator;
using Delivery.Structural.Facade;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.App;

// The only place where concrete classes are chosen and connected.
// A new service = a new factory class + one line in the list below.
public static class CompositionRoot
{
    public static (IShippingFacade Shipping, HistoryNotifier History) Create(SerilogLogger logger)
    {
        // Decorator (history) around the Adapter (Serilog).
        var history = new HistoryNotifier(new SerilogNotifierAdapter(logger));

        PricingFactory[] factories =
        {
            new StandardPricingFactory(),
            new ExpressPricingFactory(),
            new OvernightPricingFactory(),
            new InternationalPricingFactory()
        };

        IShippingFacade shipping = new ShippingFacade(
            factories,
            new DeliveryService(history),
            new LabelPrinter());

        return (shipping, history);
    }
}
