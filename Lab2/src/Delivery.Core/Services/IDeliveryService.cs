using Delivery.Domain;
using Delivery.Pricing;

namespace Delivery.Services;

// What the rest of the system needs from the delivery service.
public interface IDeliveryService
{
    Parcel Register(string recipientName, string recipientEmail, Address destination,
                    double weightKg, string serviceLabel, IPricingStrategy pricing);

    void Advance(string trackingId, ParcelStatus newStatus);

    Parcel Get(string trackingId);
}
