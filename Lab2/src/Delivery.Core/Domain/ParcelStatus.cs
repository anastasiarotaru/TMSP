namespace Delivery.Domain;

// The statuses a parcel goes through, in order.
public enum ParcelStatus
{
    Registered,
    PickedUp,
    InTransit,
    Delivered
}
