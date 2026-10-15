"""Strategii de preț: exemplu de Open/Closed Principle (OCP).

"Entitatile software trebuie sa fie deschise pentru extindere, dar inchise pentru modificare."

DeliveryService lucreaza cu orice PricingStrategy. Ca sa adaugi un serviciu nou
(de exemplu "Economy" sau "Fragile") scrii o clasa NOUA si o inregistrezi.
Codul existent nu se modifica.

Anti-pattern: un if/elif de forma `if service == "express": ... elif service == "overnight": ...`
in serviciul principal, care trebuie editat la fiecare serviciu nou.
"""

from __future__ import annotations

from abc import ABC, abstractmethod


class PricingStrategy(ABC):
    """Abstractie: primeste greutatea si returneaza pretul in lei."""

    @abstractmethod
    def price(self, weight_kg: float) -> float:
        ...


class StandardPricing(PricingStrategy):
    """5 lei taxa de baza + 1.50 lei pentru fiecare kg."""

    BASE_FEE = 5.0
    PER_KG = 1.5

    def price(self, weight_kg: float) -> float:
        return round(self.BASE_FEE + self.PER_KG * weight_kg, 2)


class ExpressPricing(PricingStrategy):
    """Livrare rapida: pretul standard inmultit cu un factor (implicit 2)."""

    def __init__(self, base: PricingStrategy, multiplier: float = 2.0) -> None:
        self.base = base
        self.multiplier = multiplier

    def price(self, weight_kg: float) -> float:
        return round(self.base.price(weight_kg) * self.multiplier, 2)


class OvernightPricing(PricingStrategy):
    """Livrare in ziua urmatoare: pretul de baza plus un supliment fix."""

    def __init__(self, base: PricingStrategy, surcharge: float = 15.0) -> None:
        self.base = base
        self.surcharge = surcharge

    def price(self, weight_kg: float) -> float:
        return round(self.base.price(weight_kg) + self.surcharge, 2)
