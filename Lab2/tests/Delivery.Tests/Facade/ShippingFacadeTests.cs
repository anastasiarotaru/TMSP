using Delivery.Creational;
using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Structural.Decorator;
using Delivery.Structural.Facade;
using Delivery.Tests.Stubs;

namespace Delivery.Tests.Facade;

public class ShippingFacadeTests
{
    private readonly CallLog log = new();
    private readonly StubDeliveryService delivery;
    private readonly IShippingFacade facade;

    public ShippingFacadeTests()
    {
        delivery = new StubDeliveryService(log);
        facade = new ShippingFacade(
            new PricingFactory[] { new StandardPricingFactory(), new ExpressPricingFactory() },
            delivery,
            new StubLabelPrinter(log));
    }

    private static ShipmentRequest Request(string service, params IPricingOption[] options) =>
        new("Ana", "ana@example.com", new Address("Str. 1", "Chisinau"), 3.2, service, options);

    [Fact]
    public void Ship_RegistersTheParcelFirst_ThenPrintsTheLabel()
    {
        facade.Ship(Request("express"));

        Assert.Equal(new[] { "Register:express", "PrintLabel:TEST01" }, log.Calls);
    }

    [Fact]
    public void Ship_AppliesTheOptionsAsDecorators_BeforeRegistering()
    {
        Parcel parcel = facade.Ship(Request("express", new InsuranceOption(1000), new FragileOption()));

        Assert.Equal("express + insurance + fragile", parcel.Service);
        Assert.IsType<FragileDecorator>(delivery.LastPricing);
        Assert.Equal(49.60, parcel.Price, 2);
    }

    [Fact]
    public void Ship_UnknownService_ThrowsAndDoesNotTouchTheSubsystem()
    {
        Assert.Throws<ArgumentException>(() => facade.Ship(Request("teleport")));
        Assert.Empty(log.Calls);
    }

    [Fact]
    public void DeliverToEnd_AdvancesThroughTheRemainingStagesInOrder()
    {
        Parcel parcel = facade.Ship(Request("standard"));
        log.Calls.Clear();

        facade.DeliverToEnd(parcel.TrackingId);

        Assert.Equal(new[] { "Get", "Advance:PickedUp", "Advance:InTransit", "Advance:Delivered" }, log.Calls);
    }

    [Fact]
    public void NewService_IsAddedByPassingANewFactory_WithoutChangingTheFacade()
    {
        var extended = new ShippingFacade(
            new PricingFactory[] { new StandardPricingFactory(), new InternationalPricingFactory() },
            delivery,
            new StubLabelPrinter(log));

        Assert.Contains("international", extended.ServiceNames);
        Assert.Equal(250.38, extended.Ship(Request("international")).Price, 2);
    }
}
