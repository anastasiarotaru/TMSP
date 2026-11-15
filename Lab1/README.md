# Laboratory Work 1 - SOLID Principles (package delivery system)

## Purpose

The purpose of this laboratory work is to analyse the SOLID principles and implement three of them in a simple package delivery application. The chosen principles are SRP (Single Responsibility Principle), OCP (Open/Closed Principle) and DIP (Dependency Inversion Principle).

The application registers a parcel with a recipient, an address, a weight and a delivery service (standard, express or overnight). The program calculates the price, prints the label and moves the parcel through the delivery stages, sending a notification to the customer at every change.

## What SOLID is

SOLID is a set of five principles used in object-oriented programming to get code that is better organised, easier to understand and simpler to change. The principles help reduce the dependencies between classes and separate responsibilities.

Not all five principles were implemented in this laboratory work. SRP, OCP and DIP were selected because they fit the delivery domain naturally: there is parcel data, several delivery services with different prices and several ways to notify the customer.

## How to run

The .NET SDK must be installed. The project uses .NET 10.

From PowerShell run:

```powershell
cd Lab1
dotnet run
```

To check that the project compiles without starting the program:

```powershell
dotnet build
```

### Using the program

The program asks, one by one, for the recipient's name, e-mail, street, city, weight (in kg) and service. The accepted services are `standard`, `express` and `overnight`. The weight must be between 0 and 30 kg. The program's messages are in Romanian.

At the end, the program prints the parcel label and then moves it through the stages `picked up`, `in transit` and `delivered`, with a notification for each stage.

An example interaction:

```text
Numele destinatarului: Ion Popescu
E-mail destinatar: ion@example.com
Strada: str. Stefan cel Mare 10
Oras: Chisinau
Greutate (kg): 3,2
Serviciu (standard / express / overnight): express
```

For this example, the standard price is 5 + 1.5 x 3.2 = 9.80 lei, and the express service doubles the price, so the final amount is 19.60 lei.

## Implementation of the SOLID principles

### 1. SRP - Single Responsibility Principle

SRP means that a class should have a single main responsibility and a single important reason to change.

In the project, SRP is implemented in `SRP.cs`. The `Parcel` class holds the parcel data, the `Address` class holds the address, and `LabelPrinter` is responsible for printing the label. The `Parcel` class does not calculate the price and does not print anything.

The responsibilities are separated: if the label layout changes, only `LabelPrinter` is modified; if the parcel data changes, only `Parcel` is modified.

### 2. OCP - Open/Closed Principle

OCP means that a module should be open for extension but closed for modification. This way, new behaviour can be added without changing the existing code.

`OCP.cs` contains the `IPricingStrategy` interface, which defines the method `CalculatePrice(double weightKg)`. It is implemented by `StandardPricing` (5 lei + 1.50 lei/kg), `ExpressPricing` (double the standard price) and `OvernightPricing` (the standard price plus 15 lei).

To add a new service, for example `EconomyPricing`, you write a new class that implements `IPricingStrategy` and add one line to the dictionary in `main.cs`. The `DeliveryService` class does not change.

### 3. DIP - Dependency Inversion Principle

DIP means that high-level modules should not depend on concrete details, but on abstractions.

In `DIP.cs`, the `DeliveryService` class (the business logic) receives an `INotifier` and a dictionary of `IPricingStrategy` through its constructor. It does not know whether the messages go to the console or by e-mail. `main.cs` decides which implementations are used (`ConsoleNotifier`, `StandardPricing` etc.), and `InMemoryNotifier` can be used instead of the console for checks.

## Project structure

The `main.cs` file is the entry point and contains the interactive flow of the application. `SRP.cs` contains the classes for the parcel, the address and the label, `OCP.cs` contains the pricing strategies, and `DIP.cs` contains the notification interface and `DeliveryService`. The .NET project configuration is in `TMPS.csproj`.

The application flow starts in `main.cs`: the data is read from the user, `DeliveryService.Register` calculates the price and saves the parcel, `LabelPrinter` prints the label, and `DeliveryService.Advance` moves the parcel through the stages.

## Verification and testing

Testing is done by running the application interactively. The following must be checked:
- the price for each service (standard, express, overnight);
- the rejection of an unknown service and of a weight outside the 0-30 kg range;
- the order of the stages: a parcel cannot skip a stage and cannot be advanced after `delivered`.

The `dotnet build` command checks that the C# files compile, and `dotnet run` starts the application. The project does not contain automated tests yet.

## Possible extensions

For a new delivery service, a new class that implements `IPricingStrategy` can be created. For a new notification channel (e-mail, SMS), a class that implements `INotifier` can be created.

As future improvements, the application could save the parcels to a file or a database, could use `decimal` instead of `double` for money amounts and could accept several parcels in a single session.

## Conclusion

This laboratory work shows how the SOLID principles make code clearer, easier to maintain and simpler to extend. Separating responsibilities through SRP, using pricing strategies through OCP and depending on abstractions through DIP reduce the coupling between components. The application can be extended with new delivery services or notification channels without major changes to the existing classes.
