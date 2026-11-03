using DeliverySOLID;

IPricingStrategy standard = new StandardPricing();
var services = new Dictionary<string, IPricingStrategy>(StringComparer.OrdinalIgnoreCase)
{
    { "standard", standard },
    { "express", new ExpressPricing(standard) },
    { "overnight", new OvernightPricing(standard) }
};

var delivery = new DeliveryService(new ConsoleNotifier(), services);
var labels = new LabelPrinter();

Console.WriteLine("=== SERVICIUL DE LIVRARE A COLETELOR ===");
Console.WriteLine("--------------------------------------------------------------");

Console.Write("Numele destinatarului: ");
string name = Console.ReadLine() ?? "";
Console.Write("E-mail destinatar: ");
string email = Console.ReadLine() ?? "";
Console.Write("Strada: ");
string street = Console.ReadLine() ?? "";
Console.Write("Oras: ");
string city = Console.ReadLine() ?? "";
Console.Write("Greutate (kg): ");
if (!double.TryParse(Console.ReadLine(), out double weight))
{
    Console.WriteLine("Eroare: greutatea trebuie sa fie un numar.");
    return;
}
Console.Write("Serviciu (standard / express / overnight): ");
string service = Console.ReadLine() ?? "";

Parcel parcel;
try
{
    parcel = delivery.Register(name, email, new Address(street, city), weight, service);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Eroare: {ex.Message}");
    return;
}

labels.PrintLabel(parcel);

Console.WriteLine("\n=== PARCURGEREA ETAPELOR ===");
delivery.Advance(parcel.TrackingId, ParcelStatus.PickedUp);
delivery.Advance(parcel.TrackingId, ParcelStatus.InTransit);
delivery.Advance(parcel.TrackingId, ParcelStatus.Delivered);

Console.WriteLine($"\nStatus final: {parcel.Status}");
