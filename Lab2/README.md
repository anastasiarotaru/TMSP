# Laboratory Work 2 - Structural Design Patterns (Package Delivery System)

## Purpose

The purpose of this laboratory work is to study the structural design patterns and use them to extend the package delivery application from Laboratory Work 1. Three patterns were implemented: **Adapter**, **Decorator** and **Facade**.

The new features are:
- an `international` delivery service, offered by a partner courier with a different API (**Adapter**);
- paid extras for a parcel - insurance and fragile handling - and a notification log (**Decorator**);
- a simple interface through which the client sends a parcel without knowing the details of the system (**Facade**).

## What structural patterns are

Structural patterns describe how classes and objects are combined into larger structures while keeping the code flexible and easy to extend. They help connect incompatible components, add behaviour without modifying existing classes and hide the complexity of a subsystem.

## How to run

The .NET SDK must be installed. The project uses .NET 10.

```powershell
cd Lab2
dotnet run
```

The program asks for the recipient's details, the weight, the service (`standard`, `express`, `overnight`, `international`), whether the parcel is fragile and the declared value for insurance (0 = no insurance). The program's messages are in Romanian.

Example:

```text
Numele destinatarului: Ion Popescu
E-mail destinatar: ion@example.com
Strada: str. Stefan cel Mare 10
Oras: Chisinau
Greutate (kg): 3,2
Serviciu (standard / express / overnight / international): express
Colet fragil? (da / nu): da
Valoare declarata pentru asigurare (lei, 0 = fara asigurare): 1000
```

The price is calculated as follows: express = 2 x (5 + 1.5 x 3.2) = 19.60 lei; insurance = 2% x 1000 = 20 lei; fragile = 10 lei. Total: **49.60 lei**.

## Implementation of the patterns

### 1. Adapter

**File:** `Adapter.cs`

The `PartnerCourierApi` class simulates the API of an external courier. Its interface is incompatible with `IPricingStrategy`: it receives the weight in grams and a zone code and returns the price in euro cents.

`PartnerPricingAdapter` implements `IPricingStrategy` and translates the call: it converts kilograms to grams, calls the API and converts the euro cents to lei.

```csharp
public double CalculatePrice(double weightKg)
{
    int grams = (int)Math.Ceiling(weightKg * 1000);
    int cents = api.QuoteInEuroCents(grams, zoneCode);
    return Math.Round(cents / 100.0 * LeiPerEuro, 2);
}
```

**Motivation:** the external service can be used like any other pricing strategy. `DeliveryService` was not modified and does not know that an external system is used behind it.

### 2. Decorator

**File:** `Decorator.cs`

`PricingDecorator` implements `IPricingStrategy` and holds another strategy, to which it delegates the calculation. The concrete decorators each add one cost:
- `InsuranceDecorator` - 2% of the declared value, minimum 3 lei;
- `FragileDecorator` - a fixed fee of 10 lei.

The decorators can be combined in any order and on top of any service:

```csharp
IPricingStrategy pricing = new ExpressPricing(new StandardPricing());
pricing = new InsuranceDecorator(pricing, 1000);
pricing = new FragileDecorator(pricing);
```

The same pattern is used for notifications: `NotifierDecorator` implements `INotifier`, and `LoggingNotifier` saves every message, with its time, in a log and then passes it on to the decorated notifier (for example `ConsoleNotifier`).

**Motivation:** the extras are added at runtime, without a subclass for every combination (express + fragile, overnight + insurance + fragile, etc.) and without modifying the existing strategies.

### 3. Facade

**File:** `Facade.cs`

`ShippingFacade` hides the whole subsystem: the configuration of the services (including the adapter), the composition of the decorators, `DeliveryService`, `LabelPrinter` and `LoggingNotifier`. The client uses only a few members:
- `Ship(ShipmentRequest)` - applies the extras, registers the parcel and prints the label;
- `DeliverToEnd(trackingId)` - moves the parcel through all stages up to `Delivered`;
- `ServiceNames` and `NotificationHistory` - for display.

```csharp
var shipping = new ShippingFacade();
Parcel parcel = shipping.Ship(new ShipmentRequest(
    name, email, street, city, weight, service, fragile, declaredValue));
shipping.DeliverToEnd(parcel.TrackingId);
```

**Motivation:** `main.cs` only reads the input and displays the results. If the way the services are built changes, only the facade is modified.

## Changes to the code from Laboratory Work 1

`DeliveryService` received an overload of the `Register` method that accepts an `IPricingStrategy` directly. This way, the facade can pass a decorated strategy. The old method (by service name) is still available and calls the new method internally.

## Project structure

- `SRP.cs`, `OCP.cs`, `DIP.cs` - code taken from Laboratory Work 1 (`../Lab1`);
- `Adapter.cs` - `PartnerCourierApi`, `PartnerPricingAdapter`;
- `Decorator.cs` - `PricingDecorator`, `InsuranceDecorator`, `FragileDecorator`, `NotifierDecorator`, `LoggingNotifier`;
- `Facade.cs` - `ShipmentRequest`, `ShippingFacade`;
- `main.cs` - the entry point, which uses the facade.

## Testing

Run the application and check:
- the price for `international` (3.2 kg -> 250.38 lei);
- the price with insurance and/or fragile handling added on top of each service;
- the selected extras shown on the label (e.g. `express + asigurare + fragil`);
- the notification log at the end of the program;
- that an unknown service is rejected.

## Conclusion

The structural patterns made it possible to extend the application without major changes to the existing code. The adapter integrated an incompatible external service, the decorators added extras and logging that can be combined freely, and the facade simplified the use of the whole system from the client's side.
