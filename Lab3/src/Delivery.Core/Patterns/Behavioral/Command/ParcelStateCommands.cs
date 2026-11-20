using Delivery.Behavioral.State;
using Delivery.Services;

namespace Delivery.Behavioral.Command;

// Base for commands that move a parcel to another state. It remembers the state
// object from before, so Undo can put it back. The real work is done by the receiver.
public abstract class ParcelStateCommand : ICommand
{
    protected readonly IDeliveryService receiver;
    protected readonly string trackingId;
    private IParcelState? previous;

    protected ParcelStateCommand(IDeliveryService receiver, string trackingId)
    {
        this.receiver = receiver;
        this.trackingId = trackingId;
    }

    public abstract string Description { get; }

    public void Execute()
    {
        IParcelState before = receiver.Get(trackingId).State;
        Apply();
        previous = before;   // saved only if Apply() did not throw
    }

    public void Undo()
    {
        if (previous is null)
        {
            throw new InvalidOperationException("The command was not executed.");
        }

        receiver.RestoreState(trackingId, previous);
        previous = null;
    }

    protected abstract void Apply();
}

// Moves the parcel to its next stage.
public class AdvanceParcelCommand : ParcelStateCommand
{
    public AdvanceParcelCommand(IDeliveryService receiver, string trackingId) : base(receiver, trackingId) { }

    public override string Description => $"advance {trackingId}";

    protected override void Apply() => receiver.Advance(trackingId);
}

// Cancels the shipment.
public class CancelParcelCommand : ParcelStateCommand
{
    public CancelParcelCommand(IDeliveryService receiver, string trackingId) : base(receiver, trackingId) { }

    public override string Description => $"cancel {trackingId}";

    protected override void Apply() => receiver.Cancel(trackingId);
}
