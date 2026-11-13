using DeliverySOLID;
using DeliveryStructural;

var shipping = new ShippingFacade();

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
Console.Write($"Serviciu ({string.Join(" / ", shipping.ServiceNames)}): ");
string service = Console.ReadLine() ?? "";
Console.Write("Colet fragil? (da / nu): ");
bool fragile = (Console.ReadLine() ?? "").Trim().Equals("da", StringComparison.OrdinalIgnoreCase);
Console.Write("Valoare declarata pentru asigurare (lei, 0 = fara asigurare): ");
if (!double.TryParse(Console.ReadLine(), out double declaredValue) || declaredValue < 0)
{
    Console.WriteLine("Eroare: valoarea declarata trebuie sa fie un numar pozitiv sau 0.");
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
    Console.WriteLine($"Eroare: {ex.Message}");
    return;
}

Console.WriteLine("\n=== PARCURGEREA ETAPELOR ===");
shipping.DeliverToEnd(parcel.TrackingId);

Console.WriteLine($"\nStatus final: {parcel.Status}");

Console.WriteLine("\n=== JURNAL NOTIFICARI ===");
foreach (string entry in shipping.NotificationHistory)
{
    Console.WriteLine(entry);
}
