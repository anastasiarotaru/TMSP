# Laboratory 1 - SOLID Principles (Parcel Delivery System)

## Purpose of the Laboratory

The purpose of this laboratory is to analyze the SOLID principles and implement three of them in a simple parcel delivery application. The selected principles are SRP (Single Responsibility Principle), OCP (Open/Closed Principle) and DIP (Dependency Inversion Principle).

The application registers a parcel with a recipient, an address, a weight and a delivery service (standard, express or overnight). The program calculates the price, prints the label and moves the parcel through the delivery stages, notifying the customer at every change.

## What SOLID Means

SOLID is a set of five principles used in object-oriented programming to produce code that is better organized, easier to understand and simpler to change. The principles help reduce dependencies between classes and separate responsibilities.

Not all five principles were implemented in this laboratory. SRP, OCP and DIP were selected because they fit the delivery domain naturally: there is parcel data, there are several delivery services with different prices, and there are several ways to notify the customer.

## How to Run

The .NET SDK must be installed. The project uses .NET 10.

From PowerShell:

```powershell
cd D:\TMPS\Lab1
dotnet run
```

To check that the project compiles without starting the program:

```powershell
dotnet build
```

### Using the Program

The program asks, in order, for the recipient's name, e-mail, street, city, weight (in kg) and service. The accepted services are `standard`, `express` and `overnight`. The weight must be between 0 and 30 kg.

At the end, the program prints the parcel label and then moves it through the `picked up`, `in transit` and `delivered` stages, sending a notification for each stage.

Example interaction:

```text
Recipient name: Ion Popescu
Recipient e-mail: ion@example.com
Street: str. Stefan cel Mare 10
City: Chisinau
Weight (kg): 3,2
Service (standard / express / overnight): express
```

For this example, the standard price is 5 + 1.5 x 3.2 = 9.80 lei, and the express service doubles it, so the final amount is 19.60 lei.

## Implementation of the SOLID Principles

### 1. SRP - Single Responsibility Principle

SRP means that a class should have a single main responsibility and only one important reason to change.

In this project, SRP is implemented in `SRP.cs`. The `Parcel` class holds the parcel data, the `Address` class holds the address, and `LabelPrinter` is responsible for printing the label. `Parcel` does not calculate the price and does not print anything.

The responsibilities are separated: if the label layout changes, only `LabelPrinter` is modified; if the parcel data changes, only `Parcel` is modified.

### 2. OCP - Open/Closed Principle

OCP means that a module should be open for extension but closed for modification. This way, new behavior can be added without changing existing code.

`OCP.cs` contains the `IPricingStrategy` interface, which defines the method `CalculatePrice(double weightKg)`. It is implemented by `StandardPricing` (5 lei + 1.50 lei/kg), `ExpressPricing` (double the standard price) and `OvernightPricing` (the standard price plus 15 lei).

To add a new service, for example `EconomyPricing`, a new class implementing `IPricingStrategy` is written and one line is added to the dictionary in `main.cs`. The `DeliveryService` class does not change.

### 3. DIP - Dependency Inversion Principle

DIP means that high-level modules should not depend on concrete details, but on abstractions.

In `DIP.cs`, the `DeliveryService` class (the business logic) receives an `INotifier` and a dictionary of `IPricingStrategy` objects through its constructor. It does not know whether messages go to the console or to e-mail. `main.cs` decides which implementations are used (`ConsoleNotifier`, `StandardPricing`, etc.), and `InMemoryNotifier` can be used instead of the console for checks.

## Project Structure

`main.cs` is the entry point and contains the interactive flow of the application. `SRP.cs` contains the parcel, address and label classes, `OCP.cs` contains the pricing strategies, and `DIP.cs` contains the notification interface and `DeliveryService`. The .NET project configuration is in `TMPS.csproj`.

The application flow starts in `main.cs`: the data is read from the user, `DeliveryService.Register` calculates the price and stores the parcel, `LabelPrinter` prints the label, and `DeliveryService.Advance` moves the parcel through the stages.

## Verification and Testing

Testing is done by running the application interactively. The following should be checked:
- the price for each service (standard, express, overnight);
- rejection of an unknown service and of a weight outside the 0-30 kg range;
- the order of the stages: a parcel cannot skip a stage and cannot be advanced after `delivered`.

`dotnet build` checks that the C# files compile, and `dotnet run` starts the application. The project does not contain automated tests yet.

## Possible Extensions

A new delivery service can be added by creating a new class that implements `IPricingStrategy`. A new notification channel (e-mail, SMS) can be added by creating a class that implements `INotifier`.

Future improvements could include saving parcels to a file or a database, using `decimal` instead of `double` for money amounts, and accepting several parcels in a single session.

## Conclusion

The laboratory shows how the SOLID principles make code clearer, easier to maintain and simpler to extend. Separating responsibilities with SRP, using pricing strategies with OCP and depending on abstractions with DIP reduce the coupling between components. The application can be extended with new delivery services or notification channels without major changes to the existing classes.
