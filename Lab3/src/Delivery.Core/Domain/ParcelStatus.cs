namespace Delivery.Domain;

// The names of the parcel states, used for display and reports.
// The behavior of each state lives in its own class (Patterns/Behavioral/State).
public enum ParcelStatus
{
    Registered,
    PickedUp,
    InTransit,
    Delivered,
    Cancelled
}
