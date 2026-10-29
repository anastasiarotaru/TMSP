namespace DeliverySOLID;

// Abstraction: how a message is sent to the customer.
public interface INotifier
{
    void Send(string recipient, string message);
}

// Prints the messages to the console.
public class ConsoleNotifier : INotifier
{
    public void Send(string recipient, string message)
    {
        Console.WriteLine($"  [notification -> {recipient}] {message}");
    }
}

// Keeps the messages in memory. Useful for checks.
public class InMemoryNotifier : INotifier
{
    public List<string> Sent { get; } = new();

    public void Send(string recipient, string message)
    {
        Sent.Add($"{recipient}: {message}");
    }
}

// High-level class. It depends only on the INotifier and IPricingStrategy abstractions.
public class DeliveryService
{
    private static readonly ParcelStatus[] Flow =
    {
        ParcelStatus.Registered,
        ParcelStatus.PickedUp,
        ParcelStatus.InTransit,
        ParcelStatus.Delivered
    };

    private readonly INotifier notifier;
    private readonly Dictionary<string, IPricingStrategy> services;
    private readonly Dictionary<string, Parcel> parcels = new();

    public DeliveryService(INotifier notifier, Dictionary<string, IPricingStrategy> services)
    {
        this.notifier = notifier;
        this.services = services;
    }

    public Parcel Register(string recipientName, string recipientEmail,
                           Address destination, double weightKg, string service)
    {
        if (!services.ContainsKey(service))
        {
            throw new ArgumentException(
                $"Unknown service '{service}'. Available: {string.Join(", ", services.Keys)}");
        }

        // The Builder creates the parcel; it validates the data and gets the code from the Singleton.
        Parcel parcel = new ParcelBuilder()
            .To(recipientName, recipientEmail)
            .Destination(destination)
            .Weight(weightKg)
            .Service(service)
            .Build(services[service]);

        parcels[parcel.TrackingId] = parcel;

        notifier.Send(recipientEmail,
            $"Parcel {parcel.TrackingId} has been registered. Price: {parcel.Price:F2} lei.");
        return parcel;
    }

    // Moves the parcel to the next stage. Stages cannot be skipped.
    public void Advance(string trackingId, ParcelStatus newStatus)
    {
        Parcel parcel = Get(trackingId);
        int index = Array.IndexOf(Flow, parcel.Status);

        if (index + 1 >= Flow.Length || Flow[index + 1] != newStatus)
        {
            throw new InvalidOperationException(
                $"Invalid transition: {parcel.Status} -> {newStatus}");
        }

        parcel.Status = newStatus;
        notifier.Send(parcel.RecipientEmail,
            $"Parcel {trackingId}: new status - {newStatus}.");
    }

    public Parcel Get(string trackingId)
    {
        if (!parcels.TryGetValue(trackingId, out Parcel? parcel))
        {
            throw new KeyNotFoundException($"There is no parcel with the code {trackingId}");
        }

        return parcel;
    }
}
