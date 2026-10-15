# Proiect: livrarea pachetelor (3 principii SOLID)

Un proiect mic în Python, fără librării externe, care arată trei principii SOLID
într-un scenariu realist de livrare a coletelor.

| Principiu | Fișier | Ce arată |
|-----------|--------|----------|
| **S** – Single Responsibility | `labels.py`, `models.py` | Datele pachetului, eticheta și logica de business sunt în clase separate. Fiecare are un singur motiv de modificare. |
| **O** – Open/Closed | `pricing.py` | Strategiile de preț sunt extinse prin clase noi. `DeliveryService` nu se modifică niciodată când apare un serviciu nou. |
| **D** – Dependency Inversion | `notifications.py`, `storage.py`, `service.py` | `DeliveryService` depinde de abstracții (`Notifier`, `PackageRepository`, `PricingStrategy`), nu de implementări concrete. |

## Rulare

```bash
python3 main.py          # demo: înregistrare, etichetă, etape, istoric
python3 -m unittest -v   # teste
```

Necesită Python 3.10 sau mai nou.

## Structura

```
models.py         Package, Address, Status (doar date)
pricing.py        PricingStrategy + Standard / Express / Overnight   (OCP)
notifications.py  Notifier + ConsoleNotifier, InMemoryNotifier        (DIP)
storage.py        PackageRepository + InMemoryPackageRepository       (DIP)
labels.py         LabelPrinter - formatarea etichetei                  (SRP)
service.py        DeliveryService - coordonarea                        (SRP, OCP, DIP)
main.py           demo
test_delivery.py  teste pentru fiecare principiu și pentru fluxul de livrare
```

## Exemple de extindere

**Serviciu nou (OCP)**: scrii o clasă nouă și o adaugi în dicționarul de servicii, fără să atingi `service.py`:

```python
class EconomyPricing(PricingStrategy):
    def price(self, weight_kg: float) -> float:
        return 8.0

service = DeliveryService(repo, notifier, {
    "standard": StandardPricing(),
    "economy": EconomyPricing(),
})
```

**Alt canal de notificare (DIP)**: scrii o clasă care moștenește `Notifier` (de exemplu `EmailNotifier`) și o pasezi în constructor.

**Altă bază de date (DIP)**: scrii o clasă care moștenește `PackageRepository` (de exemplu cu SQLite).

**Altă etichetă (SRP)**: modifici doar `labels.py`.
