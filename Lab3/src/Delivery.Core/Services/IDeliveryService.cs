using Delivery.Behavioral.State;
using Delivery.Domain;
using Delivery.Pricing;

namespace Delivery.Services;

// What the rest of the system needs from the delivery service.
// It is also the Receiver of the commands.
public interface IDeliveryService
{
    Parcel Register(string recipientName, string recipientEmail, Address destination,
                    double weightKg, string serviceLabel, IPricingStrategy pricing);

    Parcel Get(string trackingId);

    void Advance(string trackingId);

    void Cancel(string trackingId);

    void ChangeDestination(string trackingId, Address newDestination);

    void RestoreState(string trackingId, IParcelState previous);
}
