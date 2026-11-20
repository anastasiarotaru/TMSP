using System.Globalization;
using Delivery.Behavioral.Command;
using Delivery.Behavioral.Observer;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Structural.Decorator;
using SerilogLogger = Serilog.ILogger;

namespace Delivery.App;

// The interactive mode: a dispatcher menu. Behavior changes while the program runs:
// commands move parcels between states, undo reverts them, observers are switched on and off.
public class InteractiveSession
{
    private record MenuItem(string Key, string Title, Action Run);

    private readonly DeliveryApp app;
    private readonly List<MenuItem> menu;
    private string? lastTrackingId;
    private bool running = true;

    private InteractiveSession(DeliveryApp app)
    {
        this.app = app;
        menu = new List<MenuItem>
        {
            new("1", "new parcel", NewParcel),
            new("2", "advance a parcel", () => RunCommand(id => new AdvanceParcelCommand(app.Delivery, id))),
            new("3", "cancel a parcel", () => RunCommand(id => new CancelParcelCommand(app.Delivery, id))),
            new("4", "change the address", () => RunCommand(id =>
                new ChangeDestinationCommand(app.Delivery, id, new Address(Ask("New street: "), Ask("New city: "))))),
            new("5", "undo the last command", Undo),
            new("6", "customer notifications on/off", () => Toggle(app.Customer, "customer notifications")),
            new("7", "status board on/off", () => Toggle(app.Board, "status board")),
            new("8", "show board, audit log and command history", Show),
            new("0", "exit", () => running = false)
        };
    }

    public static void Run(SerilogLogger logger) => new InteractiveSession(CompositionRoot.Create(logger)).Loop();

    private readonly HashSet<IParcelObserver> subscribed = new();

    private void Loop()
    {
        subscribed.Add(app.Customer);
        subscribed.Add(app.Board);

        Console.WriteLine("=== PARCEL DISPATCHER ===");
        while (running)
        {
            Console.WriteLine();
            foreach (MenuItem item in menu)
            {
                Console.WriteLine($"  {item.Key}. {item.Title}");
            }

            string key = Ask("> ").Trim();
            MenuItem? chosen = menu.Find(m => m.Key == key);
            if (chosen is null)
            {
                Console.WriteLine("Unknown option.");
                continue;
            }

            try
            {
                chosen.Run();
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void NewParcel()
    {
        string name = Ask("Recipient name: ");
        string email = Ask("Recipient e-mail: ");
        var address = new Address(Ask("Street: "), Ask("City: "));

        if (!TryReadNumber(Ask("Weight (kg): "), out double weight))
        {
            Console.WriteLine("Error: the weight must be a number.");
            return;
        }

        string service = Ask($"Service ({string.Join(" / ", app.Shipping.ServiceNames)}): ");
        var options = new List<IPricingOption>();

        if (Ask("Fragile parcel? (yes / no): ").Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            options.Add(new FragileOption());
        }

        if (TryReadNumber(Ask("Declared value for insurance (lei, 0 = no insurance): "), out double value) && value > 0)
        {
            options.Add(new InsuranceOption(value));
        }

        Parcel parcel = app.Shipping.Ship(new ShipmentRequest(name, email, address, weight, service, options));
        lastTrackingId = parcel.TrackingId;
    }

    private void RunCommand(Func<string, ICommand> create)
    {
        string input = Ask($"AWB (enter = {lastTrackingId ?? "none"}): ").Trim();
        string? id = input.Length > 0 ? input.ToUpperInvariant() : lastTrackingId;
        if (id is null)
        {
            Console.WriteLine("There is no parcel yet.");
            return;
        }

        app.Commands.Execute(create(id));
        lastTrackingId = id;
        Console.WriteLine($"{id}: {app.Delivery.Get(id).Status}, {app.Delivery.Get(id).Destination}");
    }

    private void Undo()
    {
        if (!app.Commands.Undo())
        {
            Console.WriteLine("Nothing to undo.");
        }
    }

    private void Toggle(IParcelObserver observer, string name)
    {
        if (subscribed.Remove(observer))
        {
            app.Events.Unsubscribe(observer);
            Console.WriteLine($"{name}: OFF");
        }
        else
        {
            subscribed.Add(observer);
            app.Events.Subscribe(observer);
            Console.WriteLine($"{name}: ON");
        }
    }

    private void Show()
    {
        Console.WriteLine("Status board:");
        foreach (var (status, count) in app.Board.Counts)
        {
            Console.WriteLine($"  {status,-11} {count}");
        }

        Console.WriteLine("Audit log:");
        app.Audit.Entries.ToList().ForEach(e => Console.WriteLine("  " + e));

        Console.WriteLine("Command history:");
        app.Commands.History.ToList().ForEach(e => Console.WriteLine("  " + e));
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
