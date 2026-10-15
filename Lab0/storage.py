"""Stocarea pachetelor, prin abstractie (DIP).

DeliveryService nu stie unde sunt salvate pachetele. Azi sunt in memorie,
mâine pot fi intr-o baza de date, fara sa modificam serviciul.
"""

from __future__ import annotations

from abc import ABC, abstractmethod

from models import Package


class PackageRepository(ABC):
    @abstractmethod
    def save(self, package: Package) -> None:
        ...

    @abstractmethod
    def get(self, tracking_id: str) -> Package:
        ...


class InMemoryPackageRepository(PackageRepository):
    def __init__(self) -> None:
        self._packages: dict[str, Package] = {}

    def save(self, package: Package) -> None:
        self._packages[package.tracking_id] = package

    def get(self, tracking_id: str) -> Package:
        try:
            return self._packages[tracking_id]
        except KeyError:
            raise KeyError(f"Nu exista niciun pachet cu codul {tracking_id}") from None
