"""Notificari catre clienti: exemplu de Dependency Inversion Principle (DIP).

DeliveryService (nivel inalt, logica de business) depinde doar de abstractia Notifier.
Nu stie daca mesajul ajunge pe consola, pe e-mail sau pe SMS.
"""

from __future__ import annotations

from abc import ABC, abstractmethod


class Notifier(ABC):
    @abstractmethod
    def send(self, recipient: str, message: str) -> None:
        ...


class ConsoleNotifier(Notifier):
    """Afiseaza mesajele in terminal. Potrivit pentru demo."""

    def send(self, recipient: str, message: str) -> None:
        print(f"  [notificare -> {recipient}] {message}")


class InMemoryNotifier(Notifier):
    """Pastreaza mesajele intr-o lista. Folosit in teste."""

    def __init__(self) -> None:
        self.sent: list[tuple[str, str]] = []

    def send(self, recipient: str, message: str) -> None:
        self.sent.append((recipient, message))
