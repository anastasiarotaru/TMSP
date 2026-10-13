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

Console.WriteLine("=== PARCEL DELIVERY SERVICE ===");
Console.WriteLine("--------------------------------------------------------------");

Console.Write("Recipient name: ");
string name = Console.ReadLine() ?? "";
Console.Write("Recipient e-mail: ");
string email = Console.ReadLine() ?? "";
Console.Write("Street: ");
string street = Console.ReadLine() ?? "";
Console.Write("City: ");
string city = Console.ReadLine() ?? "";
Console.Write("Weight (kg): ");
if (!double.TryParse(Console.ReadLine(), out double weight))
{
    Console.WriteLine("Error: the weight must be a number.");
    return;
}
Console.Write("Service (standard / express / overnight): ");
string service = Console.ReadLine() ?? "";

Parcel parcel;
try
{
    parcel = delivery.Register(name, email, new Address(street, city), weight, service);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return;
}

labels.PrintLabel(parcel);

Console.WriteLine("\n=== DELIVERY STAGES ===");
delivery.Advance(parcel.TrackingId, ParcelStatus.PickedUp);
delivery.Advance(parcel.TrackingId, ParcelStatus.InTransit);
delivery.Advance(parcel.TrackingId, ParcelStatus.Delivered);

Console.WriteLine($"\nFinal status: {parcel.Status}");
