### Structural Design Patterns Report  
**Author:** Rotaru Anastasia, FAF - 242  

---

### **Objectives:**

- Study and understand the Structural Design Patterns.
- Extend the functionality of the previous laboratory work by identifying new features that are needed.
- Use Structural Design Patterns to implement these features, making the code more reusable, flexible and maintainable.

---

### **Domain: Package Delivery System**

The domain is the same **Package Delivery System** from the previous laboratory work. The system registers parcels, calculates delivery prices, prints shipping labels, moves each parcel through its delivery stages and notifies the customer about every change.

In this laboratory work the system was extended with three new features:

- an **international delivery service** offered by a partner courier whose API is incompatible with the system;
- **paid extras** for a parcel (insurance and fragile handling) and a **notification log**;
- a **single, simple entry point** for sending a parcel, so the client code does not need to know how the system is assembled.

The code is written in C# (.NET 10) and is located in the `Lab2/` folder. It builds on the SOLID code from `Lab1/`.

---

### **Used Structural Design Patterns:**

1. **Adapter**  
   Converts the interface of a class into another interface that the client expects. It is used to connect the partner courier API to the existing `IPricingStrategy` interface.

2. **Decorator**  
   Attaches additional behaviour to an object dynamically, by wrapping it in another object with the same interface. It is used for the pricing extras (insurance, fragile) and for logging the notifications.

3. **Facade**  
   Provides a simple interface to a complex subsystem. It is used to hide the configuration of the services, the decorators, `DeliveryService`, `LabelPrinter` and the notifier behind one class.

---

### **Implementation**

#### **Adapter**  
`PartnerCourierApi` simulates an external courier library that cannot be modified. Its interface does not match the rest of the system: it receives the weight in **grams** and a **zone code**, and returns the price in **euro cents**. The rest of the system works with kilograms and lei through `IPricingStrategy`.

`PartnerPricingAdapter` implements `IPricingStrategy` and translates the call: it converts kilograms to grams, calls the partner API and converts the euro cents to lei. Thanks to the adapter, the partner service is registered as `international` next to `standard`, `express` and `overnight`, and `DeliveryService` does not know that an external system is used behind it.

```csharp
// External API with an incompatible interface (grams in, euro cents out).
public class PartnerCourierApi
{
    public int QuoteInEuroCents(int grams, string zoneCode)
    {
        int baseCents = zoneCode == "EU" ? 900 : 1500;
        int perHundredGrams = zoneCode == "EU" ? 12 : 20;
        return baseCents + (grams + 99) / 100 * perHundredGrams;
    }
}

// Adapter: makes the partner API look like a normal pricing strategy.
public class PartnerPricingAdapter : IPricingStrategy
{
    private const double LeiPerEuro = 19.5;

    private readonly PartnerCourierApi api;
    private readonly string zoneCode;

    public PartnerPricingAdapter(PartnerCourierApi api, string zoneCode)
    {
        this.api = api;
        this.zoneCode = zoneCode;
    }

    public double CalculatePrice(double weightKg)
    {
        int grams = (int)Math.Ceiling(weightKg * 1000);
        int cents = api.QuoteInEuroCents(grams, zoneCode);
        return Math.Round(cents / 100.0 * LeiPerEuro, 2);
    }
}
```

#### **Decorator**  
The abstract class `PricingDecorator` implements `IPricingStrategy` and keeps a reference to another strategy, to which it delegates the calculation. Each concrete decorator adds one extra cost on top of the wrapped price:

- `InsuranceDecorator` adds 2% of the declared value (minimum 3 lei);
- `FragileDecorator` adds a fixed fee of 10 lei.

Because every decorator has the same interface as the object it wraps, the extras can be stacked in any combination and on top of any service, including the adapted international service. Without this pattern, a separate subclass would be needed for every combination (express + fragile, overnight + insurance + fragile, and so on).

```csharp
// Base decorator: same interface, delegates to the wrapped strategy.
public abstract class PricingDecorator : IPricingStrategy
{
    protected readonly IPricingStrategy inner;

    protected PricingDecorator(IPricingStrategy inner)
    {
        this.inner = inner;
    }

    public virtual double CalculatePrice(double weightKg) => inner.CalculatePrice(weightKg);
}

// Concrete decorator: adds a fixed fee for fragile parcels.
public class FragileDecorator : PricingDecorator
{
    private const double Fee = 10.0;

    public FragileDecorator(IPricingStrategy inner) : base(inner)
    {
    }

    public override double CalculatePrice(double weightKg) =>
        Math.Round(base.CalculatePrice(weightKg) + Fee, 2);
}
```

The same pattern is used for notifications. `NotifierDecorator` implements `INotifier`, and `LoggingNotifier` saves every message with its time in a history list before passing it to the wrapped notifier (for example `ConsoleNotifier`). The notification behaviour is extended without changing `ConsoleNotifier` or `DeliveryService`.

```csharp
public class LoggingNotifier : NotifierDecorator
{
    private readonly List<string> history = new();

    public IReadOnlyList<string> History => history;

    public LoggingNotifier(INotifier inner) : base(inner)
    {
    }

    public override void Send(string recipient, string message)
    {
        history.Add($"[{DateTime.Now:HH:mm:ss}] {recipient}: {message}");
        base.Send(recipient, message);
    }
}
```

#### **Facade**  
`ShippingFacade` hides the whole subsystem from the client. Inside its constructor it creates the pricing services (including the adapter), wraps the console notifier in `LoggingNotifier` and creates `DeliveryService`. Its `Ship` method chooses the pricing strategy, applies the requested decorators, registers the parcel and prints the label. `DeliverToEnd` moves the parcel through all remaining stages.

```csharp
public Parcel Ship(ShipmentRequest request)
{
    if (!services.TryGetValue(request.Service, out IPricingStrategy? pricing))
    {
        throw new ArgumentException(
            $"Serviciu necunoscut '{request.Service}'. Disponibile: {string.Join(", ", services.Keys)}");
    }

    string serviceLabel = request.Service.ToLowerInvariant();

    if (request.DeclaredValue > 0)
    {
        pricing = new InsuranceDecorator(pricing, request.DeclaredValue);
        serviceLabel += " + asigurare";
    }

    if (request.Fragile)
    {
        pricing = new FragileDecorator(pricing);
        serviceLabel += " + fragil";
    }

    Parcel parcel = delivery.Register(
        request.RecipientName, request.RecipientEmail,
        new Address(request.Street, request.City),
        request.WeightKg, serviceLabel, pricing);

    labels.PrintLabel(parcel);
    return parcel;
}
```

As a result, `main.cs` only reads the user input and calls the facade:

```csharp
var shipping = new ShippingFacade();

Parcel parcel = shipping.Ship(new ShipmentRequest(
    name, email, street, city, weight, service, fragile, declaredValue));

shipping.DeliverToEnd(parcel.TrackingId);
```

---

### **Important Parts of the Code:**

1. **`PartnerPricingAdapter` (Adapter)**: the only class that knows about grams, zone codes and euro cents. If the partner changes its API, only the adapter has to be modified.

2. **`PricingDecorator` and its subclasses (Decorator)**: extras are added at runtime by wrapping the chosen strategy. The existing `StandardPricing`, `ExpressPricing` and `OvernightPricing` classes were not modified.

3. **`LoggingNotifier` (Decorator)**: adds a notification history on top of any `INotifier`. The history is printed at the end of the program.

4. **`ShippingFacade` (Facade)**: the client works with three simple members (`Ship`, `DeliverToEnd`, `NotificationHistory`) instead of creating and connecting six or more objects by hand.

5. **Change to the Lab 1 code**: `DeliveryService` received a second `Register` method that accepts an `IPricingStrategy` directly, so the facade can pass a decorated strategy. The old method (by service name) still exists and calls the new one.

---

### **Results**

Example run with an express, fragile parcel of 3.2 kg, insured for 1000 lei:

```text
============================================
AWB:        RO000001
Destinatar: Ion Popescu
Adresa:     str. Stefan cel Mare 10, Chisinau
Greutate:   3.2 kg
Serviciu:   express + asigurare + fragil
Pret:       49.60 lei
Status:     Registered
============================================

=== PARCURGEREA ETAPELOR ===
  [notificare -> ion@example.com] Coletul RO000001: status nou - PickedUp.
  [notificare -> ion@example.com] Coletul RO000001: status nou - InTransit.
  [notificare -> ion@example.com] Coletul RO000001: status nou - Delivered.

Status final: Delivered
```

The price is composed as follows: express = 2 × (5 + 1.5 × 3.2) = 19.60 lei, insurance = 2% × 1000 = 20 lei, fragile = 10 lei, total **49.60 lei**. For the `international` service, the same 3.2 kg parcel costs (9.00 + 32 × 0.12) EUR × 19.5 = **250.38 lei**, calculated through the adapter. An unknown service name is rejected with an error message that lists the available services.

---

### **Conclusions**

- The **Adapter** pattern made it possible to integrate an external courier with an incompatible interface as a normal pricing strategy, without modifying `DeliveryService`.

- The **Decorator** pattern was used to add insurance and fragile-handling fees, which can be combined freely on top of any service, and to add a notification log on top of the existing notifier.

- The **Facade** pattern simplified the client code: `main.cs` now only handles input and output, while the construction and coordination of the subsystem are hidden inside `ShippingFacade`.

Together with the SOLID principles from the previous laboratory work, these structural patterns keep the system flexible: new services, extras and notification channels can be added by writing new classes, with minimal changes to the existing code.
