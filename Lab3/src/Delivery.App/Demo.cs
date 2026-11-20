using Delivery.Behavioral.Command;
using Delivery.Behavioral.Observer;
using Delivery.Creational;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Structural.Decorator;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.App;

// A scripted demo (no input needed): each behavioral pattern, then all of them together.
public static class Demo
{
    public static void Run(SerilogLogger logger)
    {
        ShowState();

        DeliveryApp app = CompositionRoot.Create(logger);
        ShowCommand(app);
        ShowObserver(app);
        ShowTogether(app);
    }

    private static void ShowState()
    {
        Title("1. STATE - the parcel asks its current state what is allowed");

        Parcel parcel = new ParcelBuilder()
            .To("Ana", "ana@example.com")
            .Destination(new Address("str. Puskin 5", "Chisinau"))
            .Weight(2)
            .Build(new StandardPricing());

        Console.WriteLine($"new parcel                     -> {parcel.Status}");
        parcel.Advance();
        Console.WriteLine($"Advance()                      -> {parcel.Status}");
        parcel.ChangeDestination(new Address("str. Ismail 20", "Chisinau"));
        Console.WriteLine($"ChangeDestination()            -> allowed, address: {parcel.Destination}");
        parcel.Advance();
        Console.WriteLine($"Advance()                      -> {parcel.Status}");

        Try("Cancel()", parcel.Cancel);
        Try("ChangeDestination()", () => parcel.ChangeDestination(new Address("x", "y")));

        parcel.Advance();
        Console.WriteLine($"Advance()                      -> {parcel.Status} (final: {parcel.IsFinal})");
        Try("Advance()", parcel.Advance);
    }

    private static void ShowCommand(DeliveryApp app)
    {
        Title("2. COMMAND - actions as objects, with history and undo");
        Parcel parcel = Ship(app, "express", new FragileOption());
        string id = parcel.TrackingId;

        Console.WriteLine("\nExecute: advance, then change the address:");
        app.Commands.Execute(new AdvanceParcelCommand(app.Delivery, id));
        app.Commands.Execute(new ChangeDestinationCommand(app.Delivery, id, new Address("bd. Dacia 30", "Chisinau")));
        Console.WriteLine($"  now: {parcel.Status}, {parcel.Destination}");

        Console.WriteLine("\nUndo twice (reverse order):");
        app.Commands.Undo();
        app.Commands.Undo();
        Console.WriteLine($"  now: {parcel.Status}, {parcel.Destination}");

        Console.WriteLine("\nA forbidden command is rejected by the state and is not added to the history:");
        app.Commands.Execute(new AdvanceParcelCommand(app.Delivery, id));
        app.Commands.Execute(new AdvanceParcelCommand(app.Delivery, id));
        Try("  cancel while InTransit", () => app.Commands.Execute(new CancelParcelCommand(app.Delivery, id)));

        Console.WriteLine("\nCommand history:");
        foreach (string entry in app.Commands.History)
        {
            Console.WriteLine("  " + entry);
        }
    }

    private static void ShowObserver(DeliveryApp app)
    {
        Title("3. OBSERVER - subscribe and unsubscribe at runtime");
        Parcel parcel = Ship(app, "standard");
        int auditBefore = app.Audit.Entries.Count;

        Console.WriteLine("\nUnsubscribe the customer notifications, then advance:");
        app.Events.Unsubscribe(app.Customer);
        app.Commands.Execute(new AdvanceParcelCommand(app.Delivery, parcel.TrackingId));
        Console.WriteLine("  (no [serilog] line: the customer observer is not subscribed)");
        Console.WriteLine($"  the audit log still got it: {app.Audit.Entries.Count - auditBefore} new entry");

        Console.WriteLine("\nSubscribe again, then cancel:");
        app.Events.Subscribe(app.Customer);
        app.Commands.Execute(new CancelParcelCommand(app.Delivery, parcel.TrackingId));
    }

    private static void ShowTogether(DeliveryApp app)
    {
        Title("4. TOGETHER - Command -> State -> Observers -> Decorator -> Adapter (Serilog)");
        Parcel parcel = Ship(app, "international", new InsuranceOption(500));

        Console.WriteLine("\nOne command; the state changes; every observer reacts:");
        app.Commands.Execute(new AdvanceParcelCommand(app.Delivery, parcel.TrackingId));

        Console.WriteLine("\nStatus board (StatusBoardObserver):");
        foreach (var (status, count) in app.Board.Counts)
        {
            Console.WriteLine($"  {status,-11} {count}");
        }

        Console.WriteLine("\nAudit log (AuditLogObserver):");
        foreach (string entry in app.Audit.Entries)
        {
            Console.WriteLine("  " + entry);
        }
    }

    private static Parcel Ship(DeliveryApp app, string service, params IPricingOption[] options)
    {
        Console.WriteLine($"Shipping a parcel ({service}) through the facade:");
        return app.Shipping.Ship(new ShipmentRequest(
            "Ion Popescu", "ion@example.com",
            new Address("str. Stefan cel Mare 10", "Chisinau"),
            3.2, service, options));
    }

    private static void Try(string label, Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"{label,-30} -> rejected: {ex.Message}");
        }
    }

    private static void Title(string text)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 78));
        Console.WriteLine(text);
        Console.WriteLine(new string('=', 78));
    }
}
