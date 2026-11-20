using Delivery.Behavioral.Observer;
using Delivery.Behavioral.State;
using Delivery.Creational;
using Delivery.Domain;
using Delivery.Pricing;

namespace Delivery.Services;

// COMMAND - Receiver: does the real work. It changes the parcel (State context)
// and publishes what happened to the subject (Observer). It does not know who listens.
public class DeliveryService : IDeliveryService
{
    private readonly IParcelEventPublisher events;
    private readonly Dictionary<string, Parcel> parcels = new();

    public DeliveryService(IParcelEventPublisher events)
    {
        this.events = events;
    }

    public Parcel Register(string recipientName, string recipientEmail, Address destination,
                           double weightKg, string serviceLabel, IPricingStrategy pricing)
    {
        // The Builder creates the parcel; it validates the data and gets the code from the Singleton.
        Parcel parcel = new ParcelBuilder()
            .To(recipientName, recipientEmail)
            .Destination(destination)
            .Weight(weightKg)
            .Service(serviceLabel)
            .Build(pricing);

        parcels[parcel.TrackingId] = parcel;

        events.Publish(new ParcelEvent(parcel, ParcelEventKind.Registered,
            $"Parcel {parcel.TrackingId} has been registered. Price: {parcel.Price:F2} lei."));
        return parcel;
    }

    public Parcel Get(string trackingId)
    {
        if (!parcels.TryGetValue(trackingId, out Parcel? parcel))
        {
            throw new KeyNotFoundException($"There is no parcel with the code {trackingId}");
        }

        return parcel;
    }

    public void Advance(string trackingId)
    {
        Parcel parcel = Get(trackingId);
        parcel.Advance();
        Publish(parcel, ParcelEventKind.StatusChanged, $"new status - {parcel.Status}.");
    }

    public void Cancel(string trackingId)
    {
        Parcel parcel = Get(trackingId);
        parcel.Cancel();
        Publish(parcel, ParcelEventKind.StatusChanged, "the shipment was cancelled.");
    }

    public void ChangeDestination(string trackingId, Address newDestination)
    {
        Parcel parcel = Get(trackingId);
        parcel.ChangeDestination(newDestination);
        Publish(parcel, ParcelEventKind.DestinationChanged, $"new delivery address - {newDestination}.");
    }

    public void RestoreState(string trackingId, IParcelState previous)
    {
        Parcel parcel = Get(trackingId);
        parcel.RestoreState(previous);
        Publish(parcel, ParcelEventKind.Reverted, $"correction, the status is again {parcel.Status}.");
    }

    private void Publish(Parcel parcel, ParcelEventKind kind, string text) =>
        events.Publish(new ParcelEvent(parcel, kind, $"Parcel {parcel.TrackingId}: {text}"));
}
