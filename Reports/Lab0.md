### SOLID Principles Report  
**Author:** Rotaru Anastasia, FAF - 242  

---

### **Objectives:**

- Get familiar with the SOLID principles.
- Choose a specific domain.
- Implement at least 2 SOLID principles for the chosen domain.

---

### **Domain: Package Delivery System**

The domain selected for this report is a **Package Delivery System**. The system registers parcels, calculates delivery prices depending on the chosen service, generates shipping labels, tracks each parcel through its delivery stages and notifies the customer about every change. The implementation follows three SOLID principles so that new delivery services, notification channels and storage types can be added with minimal changes to the existing code.

---

### **Used SOLID Principles:**

1. **Single Responsibility Principle (SRP)**  
   Each class has one reason to change: data models, label formatting and order coordination live in separate classes.

2. **Open/Closed Principle (OCP)**  
   The pricing logic is open for extension (new `PricingStrategy` subclasses) but closed for modification (`DeliveryService` never changes when a new service is added).

3. **Dependency Inversion Principle (DIP)**  
   `DeliveryService` depends on the abstractions `Notifier`, `PackageRepository` and `PricingStrategy`, not on concrete implementations.

---

### **Implementation**

#### **Single Responsibility Principle (SRP)**  
The `Package` and `Address` classes only hold data. The `LabelPrinter` class only formats the shipping label, and the `DeliveryService` only coordinates the steps of a delivery. If the label layout changes, only `labels.py` is modified; if the storage changes, the service is not touched.

```python
# Single Responsibility Principle (SRP)
# This class only formats the shipping label.
class LabelPrinter:
    WIDTH = 44

    def render(self, package: Package) -> str:
        line = "=" * self.WIDTH
        return "\n".join(
            [
                line,
                f"AWB:        {package.tracking_id}",
                f"Destinatar: {package.recipient_name}",
                f"Adresa:     {package.destination}",
                f"Greutate:   {package.weight_kg} kg",
                f"Serviciu:   {package.service}",
                f"Status:     {package.status.value}",
                line,
            ]
        )
```

#### **Open/Closed Principle (OCP)**  
All delivery prices are calculated through the abstract class `PricingStrategy`. `DeliveryService` receives a dictionary of strategies, so a new delivery service (for example `Economy`) is added by writing a new class and registering it, without editing the service.

```python
# Open/Closed Principle (OCP)
# Abstraction: every pricing rule implements this interface.
class PricingStrategy(ABC):
    @abstractmethod
    def price(self, weight_kg: float) -> float:
        ...

# Extension: a new rule is added by writing a new class.
class OvernightPricing(PricingStrategy):
    def __init__(self, base: PricingStrategy, surcharge: float = 15.0) -> None:
        self.base = base
        self.surcharge = surcharge

    def price(self, weight_kg: float) -> float:
        return round(self.base.price(weight_kg) + self.surcharge, 2)
```

#### **Dependency Inversion Principle (DIP)**  
`DeliveryService` receives its notifier, repository and pricing strategies through the constructor. It only knows the abstract types, so the console notifier can be replaced by an e-mail notifier, and the in-memory repository by a database, without changing the service.

```python
# Dependency Inversion Principle (DIP)
# The service depends on abstractions, which are injected through the constructor.
class DeliveryService:
    def __init__(
        self,
        repository: PackageRepository,
        notifier: Notifier,
        pricing: dict[str, PricingStrategy],
    ) -> None:
        self.repository = repository
        self.notifier = notifier
        self.pricing = pricing
```

---

### **Important Parts of the Code:**

1. **`LabelPrinter` (SRP)**: formats the label only. It knows nothing about prices, storage or notifications.

2. **`PricingStrategy` and its subclasses (OCP)**: `ExpressPricing` and `OvernightPricing` wrap another strategy (composition), so new rules can be combined with existing ones without modifying them.

3. **`Notifier` interface (DIP)**: the service sends status updates through this abstraction. `ConsoleNotifier` is used in the demo and `InMemoryNotifier` in the unit tests, which shows that the same service works with different implementations.

4. **Status flow validation**: `DeliveryService.advance` only allows the next stage (`registered → picked up → in transit → delivered`). This business rule lives in the service, and the storage and notification details do not influence it.

---

### **Conclusions / Results**

- The **Single Responsibility Principle (SRP)** was implemented by separating the data models, the label formatting and the delivery coordination into different classes.

- The **Open/Closed Principle (OCP)** was demonstrated by the pricing strategies: the Standard, Express and Overnight services were added as separate classes, and a fourth test-only service was added without changing `DeliveryService`.

- The **Dependency Inversion Principle (DIP)** was demonstrated by injecting the notifier, the repository and the pricing strategies into `DeliveryService`, so that the high-level logic depends only on abstractions.

The project is covered by 13 unit tests (`python3 -m unittest -v`), which check each principle and the full delivery lifecycle. By following SOLID principles, the system is maintainable, flexible and easy to extend with minimal changes to the existing code.
