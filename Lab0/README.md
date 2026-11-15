# Project: package delivery (3 SOLID principles)

A small Python project, with no external libraries, that shows three SOLID principles
in a realistic package delivery scenario.

| Principle | File | What it shows |
|-----------|------|---------------|
| **S** – Single Responsibility | `labels.py`, `models.py` | The package data, the label and the business logic are in separate classes. Each one has a single reason to change. |
| **O** – Open/Closed | `pricing.py` | Pricing strategies are extended with new classes. `DeliveryService` never changes when a new service is added. |
| **D** – Dependency Inversion | `notifications.py`, `storage.py`, `service.py` | `DeliveryService` depends on abstractions (`Notifier`, `PackageRepository`, `PricingStrategy`), not on concrete implementations. |

## Running

```bash
python3 main.py          # demo: registration, label, stages, history
python3 -m unittest -v   # tests
```

Requires Python 3.10 or newer.

## Structure

```
models.py         Package, Address, Status (data only)
pricing.py        PricingStrategy + Standard / Express / Overnight   (OCP)
notifications.py  Notifier + ConsoleNotifier, InMemoryNotifier        (DIP)
storage.py        PackageRepository + InMemoryPackageRepository       (DIP)
labels.py         LabelPrinter - label formatting                     (SRP)
service.py        DeliveryService - coordination                      (SRP, OCP, DIP)
main.py           demo
test_delivery.py  tests for each principle and for the delivery flow
```

## Extension examples

**New service (OCP)**: write a new class and add it to the services dictionary, without touching `service.py`:

```python
class EconomyPricing(PricingStrategy):
    def price(self, weight_kg: float) -> float:
        return 8.0

service = DeliveryService(repo, notifier, {
    "standard": StandardPricing(),
    "economy": EconomyPricing(),
})
```

**Another notification channel (DIP)**: write a class that inherits from `Notifier` (for example `EmailNotifier`) and pass it to the constructor.

**Another database (DIP)**: write a class that inherits from `PackageRepository` (for example using SQLite).

**Another label layout (SRP)**: modify only `labels.py`.
