using System.Globalization;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Structural.Decorator;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.App;

// The interactive mode: reads one shipment from the keyboard and sends it through the facade.
public static class InteractiveSession
{
    public static void Run(SerilogLogger logger)
    {
        var (shipping, history) = CompositionRoot.Create(logger);

        Console.WriteLine("=== PARCEL DELIVERY SERVICE ===");
        string name = Ask("Recipient name: ");
        string email = Ask("Recipient e-mail: ");
        string street = Ask("Street: ");
        string city = Ask("City: ");

        if (!TryReadNumber(Ask("Weight (kg): "), out double weight))
        {
            Console.WriteLine("Error: the weight must be a number.");
            return;
        }

        string service = Ask($"Service ({string.Join(" / ", shipping.ServiceNames)}): ");
        bool fragile = Ask("Fragile parcel? (yes / no): ").Trim()
            .Equals("yes", StringComparison.OrdinalIgnoreCase);

        if (!TryReadNumber(Ask("Declared value for insurance (lei, 0 = no insurance): "),
                out double declaredValue) || declaredValue < 0)
        {
            Console.WriteLine("Error: the declared value must be a positive number or 0.");
            return;
        }

        var options = new List<IPricingOption>();
        if (declaredValue > 0)
        {
            options.Add(new InsuranceOption(declaredValue));
        }
        if (fragile)
        {
            options.Add(new FragileOption());
        }

        Parcel parcel;
        try
        {
            parcel = shipping.Ship(new ShipmentRequest(
                name, email, new Address(street, city), weight, service, options));
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return;
        }

        Console.WriteLine("\n=== DELIVERY STAGES ===");
        shipping.DeliverToEnd(parcel.TrackingId);
        Console.WriteLine($"\nFinal status: {parcel.Status}");

        Console.WriteLine("\n=== NOTIFICATION HISTORY ===");
        foreach (string entry in history.History)
        {
            Console.WriteLine(entry);
        }
    }

    // Accepts both "3.2" and "3,2", whatever the system language is.
    private static bool TryReadNumber(string text, out double value) =>
        double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out value);

    private static string Ask(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }
}
