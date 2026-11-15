using DeliverySOLID;
using DeliveryStructural;

var shipping = new ShippingFacade();

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
Console.Write($"Service ({string.Join(" / ", shipping.ServiceNames)}): ");
string service = Console.ReadLine() ?? "";
Console.Write("Fragile parcel? (yes / no): ");
bool fragile = (Console.ReadLine() ?? "").Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);
Console.Write("Declared value for insurance (lei, 0 = no insurance): ");
if (!double.TryParse(Console.ReadLine(), out double declaredValue) || declaredValue < 0)
{
    Console.WriteLine("Error: the declared value must be a positive number or 0.");
    return;
}

Parcel parcel;
try
{
    parcel = shipping.Ship(new ShipmentRequest(
        name, email, street, city, weight, service, fragile, declaredValue));
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return;
}

Console.WriteLine("\n=== DELIVERY STAGES ===");
shipping.DeliverToEnd(parcel.TrackingId);

Console.WriteLine($"\nFinal status: {parcel.Status}");

Console.WriteLine("\n=== NOTIFICATION LOG ===");
foreach (string entry in shipping.NotificationHistory)
{
    Console.WriteLine(entry);
}
