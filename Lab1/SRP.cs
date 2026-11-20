namespace DeliverySOLID;

// The statuses a parcel goes through.
public enum ParcelStatus
{
    Registered,
    PickedUp,
    InTransit,
    Delivered
}

// Delivery address.
public class Address
{
    public string Street { get; }
    public string City { get; }

    public Address(string street, string city)
    {
        Street = street;
        City = city;
    }

    public override string ToString() => $"{Street}, {City}";
}

// The data of a parcel. It has no pricing, printing or saving logic.
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

// Prints the shipping label. Changes only if the label layout changes.
public class LabelPrinter
{
    public void PrintLabel(Parcel parcel)
    {
        Console.WriteLine("============================================");
        Console.WriteLine($"AWB:        {parcel.TrackingId}");
        Console.WriteLine($"Recipient:  {parcel.RecipientName}");
        Console.WriteLine($"Address:    {parcel.Destination}");
        Console.WriteLine($"Weight:     {parcel.WeightKg} kg");
        Console.WriteLine($"Service:    {parcel.Service}");
        Console.WriteLine($"Price:      {parcel.Price:F2} lei");
        Console.WriteLine($"Status:     {parcel.Status}");
        Console.WriteLine("============================================");
    }
}
