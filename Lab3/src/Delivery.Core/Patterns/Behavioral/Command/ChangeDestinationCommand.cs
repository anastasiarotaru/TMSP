using Delivery.Domain;
using Delivery.Services;

namespace Delivery.Behavioral.Command;

// Changes the delivery address; Undo puts the old address back.
public class ChangeDestinationCommand : ICommand
{
    private readonly IDeliveryService receiver;
    private readonly string trackingId;
    private readonly Address newDestination;
    private Address? oldDestination;

    public ChangeDestinationCommand(IDeliveryService receiver, string trackingId, Address newDestination)
    {
        this.receiver = receiver;
        this.trackingId = trackingId;
        this.newDestination = newDestination;
    }

    public string Description => $"change address of {trackingId} to {newDestination}";

    public void Execute()
    {
        Address before = receiver.Get(trackingId).Destination;
        receiver.ChangeDestination(trackingId, newDestination);
        oldDestination = before;
    }

    public void Undo()
    {
        if (oldDestination is null)
        {
            throw new InvalidOperationException("The command was not executed.");
        }

        receiver.ChangeDestination(trackingId, oldDestination);
        oldDestination = null;
    }
}
