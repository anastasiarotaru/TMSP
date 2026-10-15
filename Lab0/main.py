"""Demo: un pachet parcurge toate etapele, iar apoi aratam cele trei principii in actiune."""

from labels import LabelPrinter
from models import Address, Status
from notifications import ConsoleNotifier
from pricing import ExpressPricing, OvernightPricing, StandardPricing
from service import DeliveryService
from storage import InMemoryPackageRepository


def build_service() -> DeliveryService:
    standard = StandardPricing()
    return DeliveryService(
        repository=InMemoryPackageRepository(),
        notifier=ConsoleNotifier(),
        pricing={
            "standard": standard,
            "express": ExpressPricing(standard),
            "overnight": OvernightPricing(standard),
        },
    )


def main() -> None:
    service = build_service()

    print("=== Inregistrare pachet ===")
    package = service.register(
        recipient_name="Ion Popescu",
        recipient_email="ion@example.com",
        destination=Address("str. Stefan cel Mare 10", "Chisinau"),
        weight_kg=3.2,
        service="express",
    )
    print(f"Pret: {package.price:.2f} lei\n")

    print("=== Eticheta (SRP) ===")
    print(LabelPrinter().render(package))
    print()

    print("=== Parcurgerea etapelor ===")
    for status in (Status.PICKED_UP, Status.IN_TRANSIT, Status.DELIVERED):
        service.advance(package.tracking_id, status)

    print("\n=== Istoric ===")
    for step in service.track(package.tracking_id):
        print(f"  - {step}")


if __name__ == "__main__":
    main()
