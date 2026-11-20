# Laboratory 3 - Behavioral Patterns (Parcel Delivery System)

A console application in C# (.NET 10) that extends the parcel delivery system from Laboratory 2 with three behavioral patterns: **State**, **Command** and **Observer**.

The full report (domain, UML diagrams, code snippets, tests, conclusions) is in [REPORT.md](REPORT.md).

## Requirements

- .NET SDK 10 or newer
- Internet access on the first build (to restore the NuGet packages: Serilog, xUnit)

## Build, Run and Test

All commands are run from the `Lab3` folder.

```bash
# build everything
dotnet build

# scripted demo (no input needed) - shows each pattern, then all of them together
dotnet run --project src/Delivery.App

# interactive dispatcher menu
dotnet run --project src/Delivery.App -- --interactive

# run all unit tests
dotnet test
```

The interactive menu:

```text
  1. new parcel
  2. advance a parcel
  3. cancel a parcel
  4. change the address
  5. undo the last command
  6. customer notifications on/off
  7. status board on/off
  8. show board, audit log and command history
  0. exit
```

Example session: `1` (create a parcel), `2` (advance it to PickedUp), `6` (turn customer notifications off), `2` (advance to InTransit - no notification is sent), `5` (undo - back to PickedUp), `3` (cancel), `8` (show everything).

## Patterns

| Pattern | Where | Role in the project |
|---|---|---|
| **State** | `Patterns/Behavioral/State` | `Parcel` (context) delegates to `RegisteredState`, `PickedUpState`, `InTransitState`, `DeliveredState`, `CancelledState`, which decide the allowed transitions and whether the address can change. |
| **Command** | `Patterns/Behavioral/Command` | `AdvanceParcelCommand`, `CancelParcelCommand`, `ChangeDestinationCommand` are run by `CommandInvoker`, which keeps a history and undoes them in reverse order. `DeliveryService` is the receiver. |
| **Observer** | `Patterns/Behavioral/Observer` | `ParcelTracker` (subject) notifies `CustomerNotificationObserver`, `AuditLogObserver` and `StatusBoardObserver`; they can be subscribed and unsubscribed at runtime. |
| Adapter, Decorator, Facade | `Patterns/Structural` | From Laboratory 2: Serilog adapter, pricing and notification decorators, `ShippingFacade`. |
| Factory Method, Builder, Singleton | `Patterns/Creational` | From Laboratory 1. |

Execution path through the patterns:
`menu -> CommandInvoker -> AdvanceParcelCommand -> DeliveryService -> Parcel/IParcelState -> ParcelTracker -> observers -> HistoryNotifier -> SerilogNotifierAdapter -> Serilog`.

## Project Structure

```text
Lab3/
├── Lab3.slnx
├── README.md
├── REPORT.md
├── src/
│   ├── Delivery.Core/                  class library (domain + patterns)
│   │   ├── Domain/                     Parcel (State context), Address, ParcelStatus, ShipmentRequest
│   │   ├── Pricing/                    IPricingStrategy, IPricingOption, pricing strategies
│   │   ├── Notifications/              INotifier, ConsoleNotifier, InMemoryNotifier
│   │   ├── Services/                   IDeliveryService, DeliveryService (Command receiver), LabelPrinter
│   │   └── Patterns/
│   │       ├── Creational/             PricingFactory, ParcelBuilder, TrackingIdGenerator
│   │       ├── Structural/             Adapter/, Decorator/, Facade/
│   │       └── Behavioral/
│   │           ├── State/              IParcelState, ParcelState, concrete states
│   │           ├── Command/            ICommand, concrete commands, CommandInvoker
│   │           └── Observer/           IParcelObserver, IParcelEvents, ParcelTracker, concrete observers
│   └── Delivery.App/                   console client
│       ├── Program.cs                  configures Serilog, chooses demo / interactive mode
│       ├── CompositionRoot.cs          the only place where concrete classes are connected
│       ├── Demo.cs                     scripted demo of each pattern
│       └── InteractiveSession.cs       dispatcher menu
└── tests/
    └── Delivery.Tests/                 xUnit tests (State/, Command/, Observer/, Adapter/, Decorator/, Facade/)
```

## Extending the Application

- **New command:** add a class that implements `ICommand` and a menu entry in `InteractiveSession`.
- **New observer:** add a class that implements `IParcelObserver` and subscribe it in `CompositionRoot.cs`.
- **New state:** add a `ParcelState` subclass and return it from the state that leads to it.
