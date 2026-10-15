import unittest

from labels import LabelPrinter
from models import Address, Status
from notifications import InMemoryNotifier
from pricing import ExpressPricing, OvernightPricing, PricingStrategy, StandardPricing
from service import DeliveryService
from storage import InMemoryPackageRepository

ADDRESS = Address("str. Test 1", "Chisinau")


def make_service(notifier=None, extra_pricing=None):
    standard = StandardPricing()
    pricing = {
        "standard": standard,
        "express": ExpressPricing(standard),
        "overnight": OvernightPricing(standard),
    }
    pricing.update(extra_pricing or {})
    return DeliveryService(InMemoryPackageRepository(), notifier or InMemoryNotifier(), pricing)


class SingleResponsibilityTests(unittest.TestCase):
    def test_standard_price(self):
        self.assertEqual(StandardPricing().price(2), 8.0)  # 5 + 1.5 * 2

    def test_label_contains_key_fields(self):
        service = make_service()
        package = service.register("Ion", "ion@x.md", ADDRESS, 1, "standard")
        label = LabelPrinter().render(package)
        self.assertIn(package.tracking_id, label)
        self.assertIn("Ion", label)
        self.assertIn("inregistrat", label)


class OpenClosedTests(unittest.TestCase):
    def test_express_costs_double_standard(self):
        standard = StandardPricing()
        self.assertEqual(ExpressPricing(standard).price(4), 22.0)  # 11 * 2

    def test_overnight_adds_surcharge(self):
        standard = StandardPricing()
        self.assertEqual(OvernightPricing(standard).price(4), 26.0)  # 11 + 15

    def test_new_service_without_modifying_service_class(self):
        class EconomyPricing(PricingStrategy):  # scris in afara DeliveryService
            def price(self, weight_kg):
                return 1.0

        service = make_service(extra_pricing={"economy": EconomyPricing()})
        package = service.register("Ana", "ana@x.md", ADDRESS, 10, "economy")
        self.assertEqual(package.price, 1.0)
        self.assertIn("economy", service.available_services)

    def test_unknown_service_rejected(self):
        with self.assertRaises(ValueError):
            make_service().register("Ana", "ana@x.md", ADDRESS, 1, "drone")


class DependencyInversionTests(unittest.TestCase):
    def test_customer_is_notified_on_registration(self):
        notifier = InMemoryNotifier()
        service = make_service(notifier=notifier)
        package = service.register("Ion", "ion@x.md", ADDRESS, 2, "standard")
        self.assertEqual(notifier.sent[0][0], "ion@x.md")
        self.assertIn(package.tracking_id, notifier.sent[0][1])

    def test_each_status_change_sends_notification(self):
        notifier = InMemoryNotifier()
        service = make_service(notifier=notifier)
        package = service.register("Ion", "ion@x.md", ADDRESS, 2, "standard")
        service.advance(package.tracking_id, Status.PICKED_UP)
        self.assertEqual(len(notifier.sent), 2)


class DeliveryFlowTests(unittest.TestCase):
    def test_full_lifecycle_and_history(self):
        service = make_service()
        package = service.register("Ion", "ion@x.md", ADDRESS, 2, "standard")
        for status in (Status.PICKED_UP, Status.IN_TRANSIT, Status.DELIVERED):
            service.advance(package.tracking_id, status)
        self.assertEqual(package.status, Status.DELIVERED)
        self.assertEqual(
            service.track(package.tracking_id),
            ["inregistrat", "ridicat", "in tranzit", "livrat"],
        )

    def test_cannot_skip_a_step(self):
        service = make_service()
        package = service.register("Ion", "ion@x.md", ADDRESS, 2, "standard")
        with self.assertRaises(ValueError):
            service.advance(package.tracking_id, Status.IN_TRANSIT)

    def test_cannot_advance_after_delivery(self):
        service = make_service()
        package = service.register("Ion", "ion@x.md", ADDRESS, 2, "standard")
        for status in (Status.PICKED_UP, Status.IN_TRANSIT, Status.DELIVERED):
            service.advance(package.tracking_id, status)
        with self.assertRaises(ValueError):
            service.advance(package.tracking_id, Status.DELIVERED)

    def test_invalid_weight_rejected(self):
        service = make_service()
        with self.assertRaises(ValueError):
            service.register("Ion", "ion@x.md", ADDRESS, 0, "standard")
        with self.assertRaises(ValueError):
            service.register("Ion", "ion@x.md", ADDRESS, 31, "standard")

    def test_unknown_tracking_id(self):
        with self.assertRaises(KeyError):
            make_service().track("RO999999")


if __name__ == "__main__":
    unittest.main()
