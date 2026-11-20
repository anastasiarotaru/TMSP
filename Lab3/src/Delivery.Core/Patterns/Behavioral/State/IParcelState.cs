using Delivery.Domain;

namespace Delivery.Behavioral.State;

// STATE - the interface of every parcel state.
// Each operation returns the state the parcel moves to, or throws if it is not allowed.
public interface IParcelState
{
    ParcelStatus Status { get; }

    bool IsFinal { get; }

    bool CanChangeDestination { get; }

    IParcelState Advance();

    IParcelState Cancel();
}
