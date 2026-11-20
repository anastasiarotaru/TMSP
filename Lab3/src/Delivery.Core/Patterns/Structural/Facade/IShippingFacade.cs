using Delivery.Domain;

namespace Delivery.Structural.Facade;

// The simple interface the client uses to send a parcel.
public interface IShippingFacade
{
    IEnumerable<string> ServiceNames { get; }

    Parcel Ship(ShipmentRequest request);

    void DeliverToEnd(string trackingId);
}
