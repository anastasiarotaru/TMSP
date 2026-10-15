"""Generarea etichetei de expediere: exemplu de Single Responsibility Principle (SRP).

Formatarea textului nu se afla in model, nici in serviciu. Daca eticheta
trebuie sa arate altfel, modifici doar acest fisier.
"""

from __future__ import annotations

from models import Package


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
