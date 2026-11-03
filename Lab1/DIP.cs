namespace DeliverySOLID;

public interface INotifier
{
    void Send(string recipient, string message);
}

public class ConsoleNotifier : INotifier
{
    public void Send(string recipient, string message)
    {
        Console.WriteLine($"  [notificare -> {recipient}] {message}");
    }
}

public class InMemoryNotifier : INotifier
{
    public List<string> Sent { get; } = new();

    public void Send(string recipient, string message)
    {
        Sent.Add($"{recipient}: {message}");
    }
}

public class DeliveryService
{
    private const double MaxWeightKg = 30.0;

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
    private int nextId = 1;

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
                $"Serviciu necunoscut '{service}'. Disponibile: {string.Join(", ", services.Keys)}");
        }

        if (weightKg <= 0 || weightKg > MaxWeightKg)
        {
            throw new ArgumentException($"Greutatea trebuie sa fie intre 0 si {MaxWeightKg} kg");
        }

        double price = services[service].CalculatePrice(weightKg);
        var parcel = new Parcel($"RO{nextId:D6}", recipientName, recipientEmail,
                                destination, weightKg, service, price);
        nextId++;
        parcels[parcel.TrackingId] = parcel;

        notifier.Send(recipientEmail,
            $"Coletul {parcel.TrackingId} a fost inregistrat. Pret: {price:F2} lei.");
        return parcel;
    }

    public void Advance(string trackingId, ParcelStatus newStatus)
    {
        Parcel parcel = Get(trackingId);
        int index = Array.IndexOf(Flow, parcel.Status);

        if (index + 1 >= Flow.Length || Flow[index + 1] != newStatus)
        {
            throw new InvalidOperationException(
                $"Tranzitie invalida: {parcel.Status} -> {newStatus}");
        }

        parcel.Status = newStatus;
        notifier.Send(parcel.RecipientEmail,
            $"Coletul {trackingId}: status nou - {newStatus}.");
    }

    public Parcel Get(string trackingId)
    {
        if (!parcels.TryGetValue(trackingId, out Parcel? parcel))
        {
            throw new KeyNotFoundException($"Nu exista niciun colet cu codul {trackingId}");
        }

        return parcel;
    }
}
