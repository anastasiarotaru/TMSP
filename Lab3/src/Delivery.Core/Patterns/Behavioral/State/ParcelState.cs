using Delivery.Domain;

namespace Delivery.Behavioral.State;

// Base class: by default nothing is allowed. Each concrete state overrides
// only the transitions that are valid for it.
public abstract class ParcelState : IParcelState
{
    public abstract ParcelStatus Status { get; }

    public virtual bool IsFinal => false;

    public virtual bool CanChangeDestination => false;

    public virtual IParcelState Advance() => throw Forbidden("advance");

    public virtual IParcelState Cancel() => throw Forbidden("cancel");

    protected InvalidOperationException Forbidden(string action) =>
        new($"Cannot {action} a parcel that is {Status}.");

    public override string ToString() => Status.ToString();
}
