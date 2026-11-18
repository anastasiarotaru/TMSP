using Delivery.Creational;
using Delivery.Domain;
using Delivery.Notifications;
using Delivery.Pricing;
using Delivery.Structural.Adapter;
using Delivery.Structural.Decorator;
using Delivery.Structural.Facade;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.App;

// A scripted demo (no input needed) that shows each pattern, then all of them together.
public static class Demo
{
    public static void Run(SerilogLogger logger)
    {
        ShowAdapter(logger);
        ShowDecorator();
        ShowFacade(logger);
    }

    private static void ShowAdapter(SerilogLogger logger)
    {
        Title("1. ADAPTER - INotifier on top of Serilog (third-party library)");
        Console.WriteLine("The client calls INotifier.Send(recipient, message);");
        Console.WriteLine("the adapter turns it into a structured Serilog event:\n");

        INotifier notifier = new SerilogNotifierAdapter(logger);
        notifier.Send("ana@example.com", "Your parcel is on its way.");
        notifier.Send("", "Message without a recipient (becomes a Warning).");
    }

    private static void ShowDecorator()
    {
        Title("2. DECORATOR - extra options stacked at runtime");
        const double weight = 3.2;

        IPricingStrategy express = new ExpressPricingFactory().CreatePricing();
        IPricingStrategy insured = new InsuranceDecorator(express, 1000);
        IPricingStrategy insuredFragile = new FragileDecorator(insured);
        IPricingStrategy fragileInsured = new InsuranceDecorator(new FragileDecorator(express), 1000);

        PrintPrice($"express, {weight} kg", express, weight);
        PrintPrice("+ insurance (1000 lei declared)", insured, weight);
        PrintPrice("+ fragile", insuredFragile, weight);
        PrintPrice("same options, other order", fragileInsured, weight);

        IPricingStrategy international = new FragileDecorator(new InternationalPricingFactory().CreatePricing());
        PrintPrice("international + fragile", international, weight);
    }

    private static void ShowFacade(SerilogLogger logger)
    {
        Title("3. FACADE - one call; behind it: Factory, Decorator, Builder, Singleton, Adapter");
        var (shipping, history) = CompositionRoot.Create(logger);

        var request = new ShipmentRequest(
            "Ion Popescu", "ion@example.com",
            new Address("str. Stefan cel Mare 10", "Chisinau"),
            3.2, "international",
            new IPricingOption[] { new InsuranceOption(500), new FragileOption() });

        Parcel parcel = shipping.Ship(request);

        Console.WriteLine("\nshipping.DeliverToEnd(...):");
        shipping.DeliverToEnd(parcel.TrackingId);
        Console.WriteLine($"Final status: {parcel.Status}");

        Console.WriteLine("\nHistory kept by the HistoryNotifier decorator:");
        foreach (string entry in history.History)
        {
            Console.WriteLine("  " + entry);
        }

        Console.WriteLine("\nAn unknown service is rejected by the facade:");
        try
        {
            shipping.Ship(request with { Service = "teleport" });
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("  Error: " + ex.Message);
        }
    }

    private static void PrintPrice(string label, IPricingStrategy pricing, double weight) =>
        Console.WriteLine($"{label,-36}{pricing.CalculatePrice(weight),9:F2} lei");

    private static void Title(string text)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 78));
        Console.WriteLine(text);
        Console.WriteLine(new string('=', 78));
    }
}
