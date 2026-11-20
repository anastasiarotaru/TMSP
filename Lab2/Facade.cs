using DeliverySOLID;

namespace DeliveryStructural;

public record ShipmentRequest(
    string RecipientName,
    string RecipientEmail,
    string Street,
    string City,
    double WeightKg,
    string Service,
    bool Fragile,
    double DeclaredValue);

// Facade: hides from the client how the services are configured (through factories), how the
// decorators are composed, how the parcel is registered, the label printed and the stages run.
// The client (main.cs) uses only a few simple methods.
public class ShippingFacade
{
    private readonly Dictionary<string, IPricingStrategy> services;
    private readonly LoggingNotifier notifier;
    private readonly DeliveryService delivery;
    private readonly LabelPrinter labels = new();

    public ShippingFacade()
    {
        // Each service is created by its own factory (Factory Method from Laboratory 1).
        var factories = new Dictionary<string, PricingFactory>(StringComparer.OrdinalIgnoreCase)
        {
            { "standard", new StandardPricingFactory() },
            { "express", new ExpressPricingFactory() },
            { "overnight", new OvernightPricingFactory() },
            { "international", new InternationalPricingFactory("EU") }
        };

        services = factories.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.CreatePricing(),
            StringComparer.OrdinalIgnoreCase);

        notifier = new LoggingNotifier(new ConsoleNotifier());
        delivery = new DeliveryService(notifier, services);
    }

    public IEnumerable<string> ServiceNames => services.Keys;

    public IReadOnlyList<string> NotificationHistory => notifier.History;

    // Registers the parcel (with the requested extra options) and prints the label.
    public Parcel Ship(ShipmentRequest request)
    {
        if (!services.TryGetValue(request.Service, out IPricingStrategy? pricing))
        {
            throw new ArgumentException(
                $"Unknown service '{request.Service}'. Available: {string.Join(", ", services.Keys)}");
        }

        string serviceLabel = request.Service.ToLowerInvariant();

        if (request.DeclaredValue > 0)
        {
            pricing = new InsuranceDecorator(pricing, request.DeclaredValue);
            serviceLabel += " + insurance";
        }

        if (request.Fragile)
        {
            pricing = new FragileDecorator(pricing);
            serviceLabel += " + fragile";
        }

        // DeliveryService uses the Builder, which validates the data and gets the AWB code from the Singleton.
        Parcel parcel = delivery.Register(
            request.RecipientName,
            request.RecipientEmail,
            new Address(request.Street, request.City),
            request.WeightKg,
            serviceLabel,
            pricing);

        labels.PrintLabel(parcel);
        return parcel;
    }

    // Moves the parcel through all remaining stages until delivery.
    public void DeliverToEnd(string trackingId)
    {
        Parcel parcel = delivery.Get(trackingId);

        foreach (ParcelStatus status in Enum.GetValues<ParcelStatus>())
        {
            if (status > parcel.Status)
            {
                delivery.Advance(trackingId, status);
            }
        }
    }
}
