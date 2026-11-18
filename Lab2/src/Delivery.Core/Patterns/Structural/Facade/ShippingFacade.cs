using Delivery.Creational;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Services;

namespace Delivery.Structural.Facade;

// FACADE: hides from the client how the services are created (factories), how the
// options are applied (decorators), how the parcel is registered, the label printed
// and the stages run. It only coordinates the subsystem; the work is done by the parts.
// All parts are received from outside, so they can be replaced (or stubbed in tests).
public class ShippingFacade : IShippingFacade
{
    private readonly Dictionary<string, IPricingStrategy> services;
    private readonly IDeliveryService delivery;
    private readonly ILabelPrinter labels;

    public ShippingFacade(IEnumerable<PricingFactory> factories,
                          IDeliveryService delivery,
                          ILabelPrinter labels)
    {
        services = factories.ToDictionary(
            f => f.ServiceName,
            f => f.CreatePricing(),
            StringComparer.OrdinalIgnoreCase);
        this.delivery = delivery;
        this.labels = labels;
    }

    public IEnumerable<string> ServiceNames => services.Keys;

    // Applies the requested options, registers the parcel and prints the label.
    public Parcel Ship(ShipmentRequest request)
    {
        if (!services.TryGetValue(request.Service, out IPricingStrategy? pricing))
        {
            throw new ArgumentException(
                $"Unknown service '{request.Service}'. Available: {string.Join(", ", services.Keys)}");
        }

        string serviceLabel = request.Service.ToLowerInvariant();

        foreach (IPricingOption option in request.Options)
        {
            pricing = option.Apply(pricing);
            serviceLabel += " + " + option.Label;
        }

        Parcel parcel = delivery.Register(
            request.RecipientName,
            request.RecipientEmail,
            request.Destination,
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
