# Laboratory 2 - Structural Patterns (Parcel Delivery System)

A console application in C# (.NET 10) that extends the parcel delivery system from Laboratory 1 with three structural patterns: **Adapter**, **Decorator** and **Facade**.

The full report (domain, UML diagrams, code snippets, tests, conclusions) is in [REPORT.md](REPORT.md).

## Requirements

- .NET SDK 10 or newer
- Internet access on the first build (to restore the NuGet packages: Serilog, xUnit)

## Build, Run and Test

All commands are run from the `Lab2` folder.

```bash
# build everything
dotnet build

# scripted demo (no input needed) - shows each pattern, then all of them together
dotnet run --project src/Delivery.App

# interactive mode - reads one shipment from the keyboard
dotnet run --project src/Delivery.App -- --interactive

# run all unit tests
dotnet test
```

Example for the interactive mode:

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

Price: express = 2 x (5 + 1.5 x 3.2) = 19.60 lei; insurance = 2% x 1000 = 20 lei; fragile = 10 lei. Total: **49.60 lei**.


## Patterns

| Pattern | Where | Role in the project |
|---|---|---|
| **Adapter** | `Patterns/Structural/Adapter` | `SerilogNotifierAdapter` makes the **Serilog** logging library (third-party) usable as an `INotifier`, so customer notifications go into the logging pipeline. |
| **Decorator** | `Patterns/Structural/Decorator` | `InsuranceDecorator` and `FragileDecorator` add extra costs to any pricing strategy, in any order; `HistoryNotifier` keeps a history of notifications. |
| **Facade** | `Patterns/Structural/Facade` | `ShippingFacade` hides the factories, decorators, delivery service and label printer behind `Ship` and `DeliverToEnd`. |
| Factory Method, Builder, Singleton | `Patterns/Creational` | From Laboratory 1: create the pricing strategies, build and validate the parcel, generate unique AWB codes. |

Execution path through the patterns:
`Program -> ShippingFacade -> PricingFactory -> option decorators -> DeliveryService -> ParcelBuilder/TrackingIdGenerator -> HistoryNotifier -> SerilogNotifierAdapter -> Serilog`.

## Project Structure

```text
Lab2/
├── Lab2.slnx
├── README.md
├── REPORT.md
├── src/
│   ├── Delivery.Core/                  class library (domain + patterns)
│   │   ├── Domain/                     Parcel, Address, ParcelStatus, ShipmentRequest
│   │   ├── Pricing/                    IPricingStrategy, IPricingOption, Standard/Express/Overnight/International
│   │   ├── Notifications/              INotifier, ConsoleNotifier, InMemoryNotifier
│   │   ├── Services/                   IDeliveryService, DeliveryService, ILabelPrinter, LabelPrinter
│   │   └── Patterns/
│   │       ├── Creational/             PricingFactory (+ subclasses), ParcelBuilder, TrackingIdGenerator
│   │       └── Structural/
│   │           ├── Adapter/            SerilogNotifierAdapter
│   │           ├── Decorator/          PricingDecorator, InsuranceDecorator, FragileDecorator,
│   │           │                       InsuranceOption, FragileOption, NotifierDecorator, HistoryNotifier
│   │           └── Facade/             IShippingFacade, ShippingFacade
│   └── Delivery.App/                   console client
│       ├── Program.cs                  configures Serilog, chooses demo / interactive mode
│       ├── CompositionRoot.cs          the only place where concrete classes are connected
│       ├── Demo.cs                     scripted demo of each pattern
│       └── InteractiveSession.cs       keyboard input
└── tests/
    └── Delivery.Tests/                 xUnit tests (Adapter/, Decorator/, Facade/, Stubs/)
```

## Extending the Application

- **New delivery service:** add a new `PricingFactory` subclass (and its strategy), then add it to the list in `CompositionRoot.cs`. The facade does not change.
- **New extra option:** add a new `PricingDecorator` subclass and an `IPricingOption` that applies it. The facade and `ShipmentRequest` do not change.
- **New notification channel:** add a new `INotifier` (or a new `NotifierDecorator`) and connect it in `CompositionRoot.cs`.
