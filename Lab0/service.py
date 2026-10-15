"""Logica de business pentru livrarea pachetelor.

Acest modul combina principiile:
* SRP  - face doar coordonarea: valideaza, calculeaza pretul, salveaza, notifica.
         Pretul, stocarea si notificarile sunt in alte module.
* OCP  - primeste dictionarul de strategii de preț ca parametru. Serviciile noi
         se adauga din afara, fara sa modificam aceasta clasa.
* DIP  - depinde de abstractiile Notifier, PackageRepository, PricingStrategy,
         nu de clasele concrete.
"""

from __future__ import annotations

import itertools

from models import Address, Package, Status
from notifications import Notifier
from pricing import PricingStrategy
from storage import PackageRepository


class DeliveryService:
    MAX_WEIGHT_KG = 30.0
    STATUS_FLOW = [Status.REGISTERED, Status.PICKED_UP, Status.IN_TRANSIT, Status.DELIVERED]

    def __init__(
        self,
        repository: PackageRepository,
        notifier: Notifier,
        pricing: dict[str, PricingStrategy],
    ) -> None:
        self.repository = repository
        self.notifier = notifier
        self.pricing = pricing
        self._ids = itertools.count(1)

    @property
    def available_services(self) -> list[str]:
        return sorted(self.pricing)

    def register(
        self,
        recipient_name: str,
        recipient_email: str,
        destination: Address,
        weight_kg: float,
        service: str,
    ) -> Package:
        if service not in self.pricing:
            raise ValueError(
                f"Serviciu necunoscut '{service}'. Disponibile: {', '.join(self.available_services)}"
            )
        if not 0 < weight_kg <= self.MAX_WEIGHT_KG:
            raise ValueError(f"Greutatea trebuie sa fie intre 0 si {self.MAX_WEIGHT_KG} kg")

        price = self.pricing[service].price(weight_kg)
        package = Package(
            tracking_id=f"RO{next(self._ids):06d}",
            recipient_name=recipient_name,
            recipient_email=recipient_email,
            destination=destination,
            weight_kg=weight_kg,
            service=service,
            price=price,
        )
        package.history.append(Status.REGISTERED.value)
        self.repository.save(package)
        self.notifier.send(
            recipient_email,
            f"Coletul {package.tracking_id} a fost inregistrat. Pret: {price:.2f} lei.",
        )
        return package

    def advance(self, tracking_id: str, new_status: Status) -> Package:
        """Trece pachetul in urmatoarea etapa. Nu se pot sari etape."""
        package = self.repository.get(tracking_id)
        index = self.STATUS_FLOW.index(package.status)
        if index + 1 >= len(self.STATUS_FLOW) or self.STATUS_FLOW[index + 1] != new_status:
            raise ValueError(
                f"Tranzitie invalida: {package.status.value} -> {new_status.value}"
            )

        package.status = new_status
        package.history.append(new_status.value)
        self.repository.save(package)
        self.notifier.send(
            package.recipient_email,
            f"Coletul {tracking_id}: status nou - {new_status.value}.",
        )
        return package

    def track(self, tracking_id: str) -> list[str]:
        return list(self.repository.get(tracking_id).history)
