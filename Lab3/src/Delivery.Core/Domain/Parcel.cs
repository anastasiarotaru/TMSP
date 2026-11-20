using Delivery.Behavioral.State;

namespace Delivery.Domain;

// STATE - Context. The parcel does not decide what is allowed; it asks its current state.
public class Parcel
{
    private IParcelState state = new RegisteredState();

    public string TrackingId { get; }
    public string RecipientName { get; }
    public string RecipientEmail { get; }
    public Address Destination { get; private set; }
    public double WeightKg { get; }
    public string Service { get; }
    public double Price { get; }

    public IParcelState State => state;
    public ParcelStatus Status => state.Status;
    public bool IsFinal => state.IsFinal;

    public Parcel(string trackingId, string recipientName, string recipientEmail,
                  Address destination, double weightKg, string service, double price)
    {
        TrackingId = trackingId;
        RecipientName = recipientName;
        RecipientEmail = recipientEmail;
        Destination = destination;
        WeightKg = weightKg;
        Service = service;
        Price = price;
    }

    // The context delegates to its current state; the state returns the next state.
    public void Advance() => state = state.Advance();

    public void Cancel() => state = state.Cancel();

    public void ChangeDestination(Address newDestination)
    {
        if (!state.CanChangeDestination)
        {
            throw new InvalidOperationException(
                $"The address cannot be changed when the parcel is {Status}.");
        }

        Destination = newDestination;
    }

    // Used only to undo a command: puts back a state object saved before the command ran.
    public void RestoreState(IParcelState previous) => state = previous;
}
