using Delivery.Behavioral.Observer;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Services;

namespace Delivery.Tests.Stubs;

public static class TestParcels
{
    public static readonly Address Home = new("Str. 1", "Chisinau");

    public static Parcel New() =>
        new("T1", "Ana", "ana@example.com", Home, 2, "standard", 8);

    public static Parcel Register(IDeliveryService delivery) =>
        delivery.Register("Ana", "ana@example.com", Home, 2, "standard", new StandardPricing());

    public static DeliveryService Receiver() => new(new ParcelTracker());
}
