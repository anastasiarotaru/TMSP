using Delivery.Domain;

namespace Delivery.Behavioral.Observer;

public enum ParcelEventKind
{
    Registered,
    StatusChanged,
    DestinationChanged,
    Reverted
}

// What happened to a parcel. Sent by the subject to every observer.
public record ParcelEvent(Parcel Parcel, ParcelEventKind Kind, string Description);
