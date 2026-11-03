namespace DeliverySOLID;

public enum ParcelStatus
{
    Registered,
    PickedUp,
    InTransit,
    Delivered
}

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

// Afiseaza eticheta de expediere. Se schimba doar daca se schimba layout-ul etichetei.
public class LabelPrinter
{
    public void PrintLabel(Parcel parcel)
    {
        Console.WriteLine("============================================");
        Console.WriteLine($"AWB:        {parcel.TrackingId}");
        Console.WriteLine($"Destinatar: {parcel.RecipientName}");
        Console.WriteLine($"Adresa:     {parcel.Destination}");
        Console.WriteLine($"Greutate:   {parcel.WeightKg} kg");
        Console.WriteLine($"Serviciu:   {parcel.Service}");
        Console.WriteLine($"Pret:       {parcel.Price:F2} lei");
        Console.WriteLine($"Status:     {parcel.Status}");
        Console.WriteLine("============================================");
    }
}
