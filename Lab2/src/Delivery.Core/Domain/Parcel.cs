namespace Delivery.Domain;

// The data of a parcel. It has no pricing, printing or saving logic (SRP).
public class Parcel
{
    public string TrackingId { get; }
    public string RecipientName { get; }
    public string RecipientEmail { get; }
    public Address Destination { get; }
    public double WeightKg { get; }
    public string Service { get; }
    public double Price { get; }
    public ParcelStatus Status { get; set; } = ParcelStatus.Registered;

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
}
