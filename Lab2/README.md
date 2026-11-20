# Laboratory 2 - Structural Patterns (Parcel Delivery System)

## Purpose of the Laboratory

The purpose of this laboratory is to study structural design patterns and use them to extend the parcel delivery application from Laboratory 1 (which contains the SOLID principles from Laboratory 0 and the creational patterns Factory Method, Builder and Singleton). Three patterns were implemented: **Adapter**, **Decorator** and **Facade**.

The new features are:
- an `international` delivery service, provided by a partner courier with a different API (**Adapter**);
- extra parcel options - insurance and special handling for fragile parcels - and a notification log (**Decorator**);
- a simple interface through which the client sends a parcel without knowing the details of the system (**Facade**).

## What Structural Patterns Are

Structural patterns describe how classes and objects are combined to form larger structures while keeping the code flexible and easy to extend. They help connect incompatible components, add behavior without modifying existing classes, and hide the complexity of a subsystem.

## How to Run

```powershell
dotnet run
```

The program asks for the recipient's details, the weight, the service (`standard`, `express`, `overnight`, `international`), whether the parcel is fragile (`yes` / `no`), and the declared value for insurance (0 = no insurance).

Example:

```text
Recipient name: Ion Popescu
Recipient e-mail: ion@example.com
Street: str. Stefan cel Mare 10
City: Chisinau
Weight (kg): 3,2
Service (standard / express / overnight / international): express
Fragile parcel? (yes / no): yes
Declared value for insurance (lei, 0 = no insurance): 1000
```

The price is calculated as follows: express = 2 x (5 + 1.5 x 3.2) = 19.60 lei; insurance = 2% x 1000 = 20 lei; fragile = 10 lei. Total: **49.60 lei**.

## Implementation of the Patterns

### 1. Adapter

**File:** `Adapter.cs`

The `PartnerCourierApi` class simulates the API of an external courier. Its interface is incompatible with `IPricingStrategy`: it takes the weight in grams and a zone code, and returns the price in euro cents.

`PartnerPricingAdapter` implements `IPricingStrategy` and translates the call: it converts kilograms to grams, calls the API and converts euro cents to lei.

```csharp
public double CalculatePrice(double weightKg)
{
    int grams = (int)Math.Ceiling(weightKg * 1000);
    int cents = api.QuoteInEuroCents(grams, zoneCode);
    return Math.Round(cents / 100.0 * LeiPerEuro, 2);
}
```

The adapter is created by `InternationalPricingFactory`, a new factory that extends `PricingFactory` from Laboratory 1. This way, the international service is added exactly like the other services: one new factory and one line in the factory dictionary.

```csharp
public class InternationalPricingFactory : PricingFactory
{
    public override IPricingStrategy CreatePricing() =>
        new PartnerPricingAdapter(new PartnerCourierApi(), zoneCode);
}
```

**Motivation:** the external service can be used like any other pricing strategy. `DeliveryService` does not know that a foreign system is behind it.

### 2. Decorator

**File:** `Decorator.cs`

`PricingDecorator` implements `IPricingStrategy` and wraps another strategy, to which it delegates the calculation. The concrete decorators each add one cost:
- `InsuranceDecorator` - 2% of the declared value, at least 3 lei;
- `FragileDecorator` - a fixed fee of 10 lei.

The decorators can be combined in any order and on top of any service:

```csharp
IPricingStrategy pricing = new ExpressPricing(new StandardPricing());
pricing = new InsuranceDecorator(pricing, 1000);
pricing = new FragileDecorator(pricing);
```

The same pattern is used for notifications: `NotifierDecorator` implements `INotifier`, and `LoggingNotifier` saves each message, with its time, in a log, then passes it on to the wrapped notifier (for example `ConsoleNotifier`).

**Motivation:** extra options are added at runtime, without a subclass for every combination (express + fragile, overnight + insurance + fragile, etc.) and without modifying the existing strategies.

### 3. Facade

**File:** `Facade.cs`

`ShippingFacade` hides the whole subsystem: creating the services through factories (including the adapter), composing the decorators, `DeliveryService`, `LabelPrinter` and `LoggingNotifier`. The client uses only a few methods:
- `Ship(ShipmentRequest)` - applies the extra options, registers the parcel and prints the label;
- `DeliverToEnd(trackingId)` - moves the parcel through all the stages up to `Delivered`;
- `ServiceNames` and `NotificationHistory` - for display.

```csharp
var shipping = new ShippingFacade();
Parcel parcel = shipping.Ship(new ShipmentRequest(
    name, email, street, city, weight, service, fragile, declaredValue));
shipping.DeliverToEnd(parcel.TrackingId);
```

**Motivation:** `main.cs` only reads the input and displays the results. If the way the services are built changes, only the facade is modified.

## Connection to Laboratory 1

The creational patterns from Laboratory 1 are still used:
- **Factory Method** - the facade creates all pricing strategies through factories (`StandardPricingFactory`, `ExpressPricingFactory`, `OvernightPricingFactory` and the new `InternationalPricingFactory`);
- **Builder** - `DeliveryService` builds the parcel with `ParcelBuilder`, which validates the data (name, address, maximum weight of 30 kg) and calculates the price with the strategy it receives, including a decorated one;
- **Singleton** - `ParcelBuilder` gets the tracking number (AWB) from `TrackingIdGenerator.Instance`.

Changes made to the Laboratory 1 code: `DeliveryService` received an overload of the `Register` method that accepts an `IPricingStrategy` directly. This lets the facade pass a decorated strategy to `ParcelBuilder`. The old method (by service name) was kept and uses the new method internally.

## Project Structure

- `SRP.cs`, `OCP.cs`, `DIP.cs` - the SOLID principles (Laboratory 0);
- `Factory.cs`, `Builder.cs`, `Singleton.cs` - the creational patterns (Laboratory 1);
- `Adapter.cs` - `PartnerCourierApi`, `PartnerPricingAdapter`, `InternationalPricingFactory`;
- `Decorator.cs` - `PricingDecorator`, `InsuranceDecorator`, `FragileDecorator`, `NotifierDecorator`, `LoggingNotifier`;
- `Facade.cs` - `ShipmentRequest`, `ShippingFacade`;
- `main.cs` - the entry point, which uses the facade.

## Verification

Run the application and check:
- the price for `international` (3.2 kg -> 250.38 lei);
- the price with insurance and/or fragile handling added on top of each service;
- the chosen options shown on the label (e.g. `express + insurance + fragile`);
- the notification log at the end of the program;
- rejection of an unknown service;
- rejection of a weight above 30 kg (validated by `ParcelBuilder`).

## Conclusion

The structural patterns made it possible to extend the application without major changes to the existing code. The adapter integrated an incompatible external service and is created through a factory, like the other services; the decorators added options and logging that can be combined freely; and the facade simplified the use of the whole system for the client.
