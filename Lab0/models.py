"""Modele de date pentru livrarea pachetelor.

SRP: acest modul contine doar DATELE unui pachet. Nu stie cum se calculeaza
pretul, cum se afiseaza eticheta sau cum se salveaza pachetul.
Se schimba doar daca se schimba structura datelor.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum


class Status(Enum):
    REGISTERED = "inregistrat"
    PICKED_UP = "ridicat"
    IN_TRANSIT = "in tranzit"
    DELIVERED = "livrat"


@dataclass(frozen=True)
class Address:
    street: str
    city: str

    def __str__(self) -> str:
        return f"{self.street}, {self.city}"


@dataclass
class Package:
    tracking_id: str
    recipient_name: str
    recipient_email: str
    destination: Address
    weight_kg: float
    service: str
    price: float
    status: Status = Status.REGISTERED
    history: list[str] = field(default_factory=list)
