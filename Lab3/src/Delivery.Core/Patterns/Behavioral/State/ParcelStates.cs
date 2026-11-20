using Delivery.Domain;

namespace Delivery.Behavioral.State;

// Registered: waiting for the courier. Can be picked up, cancelled, or get a new address.
public class RegisteredState : ParcelState
{
    public override ParcelStatus Status => ParcelStatus.Registered;
    public override bool CanChangeDestination => true;
    public override IParcelState Advance() => new PickedUpState();
    public override IParcelState Cancel() => new CancelledState();
}

// Picked up: in the local depot. Can leave the depot, be cancelled, or get a new address.
public class PickedUpState : ParcelState
{
    public override ParcelStatus Status => ParcelStatus.PickedUp;
    public override bool CanChangeDestination => true;
    public override IParcelState Advance() => new InTransitState();
    public override IParcelState Cancel() => new CancelledState();
}

// In transit: on the road. It can only be delivered; no cancel, no address change.
public class InTransitState : ParcelState
{
    public override ParcelStatus Status => ParcelStatus.InTransit;
    public override IParcelState Advance() => new DeliveredState();
}

// Delivered: final state, nothing more is allowed.
public class DeliveredState : ParcelState
{
    public override ParcelStatus Status => ParcelStatus.Delivered;
    public override bool IsFinal => true;
}

// Cancelled: final state, nothing more is allowed.
public class CancelledState : ParcelState
{
    public override ParcelStatus Status => ParcelStatus.Cancelled;
    public override bool IsFinal => true;
}
