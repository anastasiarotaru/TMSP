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

// Facade: ascunde de client modul in care se configureaza serviciile, se compun
// decoratorii, se inregistreaza coletul, se tipareste eticheta si se parcurg etapele.
// Clientul (main.cs) foloseste doar cateva metode simple.
public class ShippingFacade
{
    private readonly Dictionary<string, IPricingStrategy> services;
    private readonly LoggingNotifier notifier;
    private readonly DeliveryService delivery;
    private readonly LabelPrinter labels = new();

    public ShippingFacade()
    {
        IPricingStrategy standard = new StandardPricing();
        services = new Dictionary<string, IPricingStrategy>(StringComparer.OrdinalIgnoreCase)
        {
            { "standard", standard },
            { "express", new ExpressPricing(standard) },
            { "overnight", new OvernightPricing(standard) },
            { "international", new PartnerPricingAdapter(new PartnerCourierApi(), "EU") }
        };

        notifier = new LoggingNotifier(new ConsoleNotifier());
        delivery = new DeliveryService(notifier, services);
    }

    public IEnumerable<string> ServiceNames => services.Keys;

    public IReadOnlyList<string> NotificationHistory => notifier.History;

    // Inregistreaza coletul (cu optiunile extra cerute) si afiseaza eticheta.
    public Parcel Ship(ShipmentRequest request)
    {
        if (!services.TryGetValue(request.Service, out IPricingStrategy? pricing))
        {
            throw new ArgumentException(
                $"Serviciu necunoscut '{request.Service}'. Disponibile: {string.Join(", ", services.Keys)}");
        }

        string serviceLabel = request.Service.ToLowerInvariant();

        if (request.DeclaredValue > 0)
        {
            pricing = new InsuranceDecorator(pricing, request.DeclaredValue);
            serviceLabel += " + asigurare";
        }

        if (request.Fragile)
        {
            pricing = new FragileDecorator(pricing);
            serviceLabel += " + fragil";
        }

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

    // Trece coletul prin toate etapele ramase pana la livrare.
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
