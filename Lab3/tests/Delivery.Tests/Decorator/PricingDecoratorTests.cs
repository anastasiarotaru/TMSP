using Delivery.Pricing;
using Delivery.Structural.Decorator;
using Delivery.Tests.Stubs;

namespace Delivery.Tests.Decorator;

public class PricingDecoratorTests
{
    private const double Weight = 3.2;

    [Fact]
    public void Decorators_StackedInAnyOrder_GiveTheSamePrice()
    {
        IPricingStrategy express = new ExpressPricing(new StandardPricing());

        double insuranceThenFragile = new FragileDecorator(new InsuranceDecorator(express, 1000)).CalculatePrice(Weight);
        double fragileThenInsurance = new InsuranceDecorator(new FragileDecorator(express), 1000).CalculatePrice(Weight);

        Assert.Equal(49.60, insuranceThenFragile, 2);
        Assert.Equal(insuranceThenFragile, fragileThenInsurance, 2);
    }

    [Fact]
    public void EachDecorator_AddsExactlyItsOwnFee()
    {
        IPricingStrategy basePrice = new FixedPricing(100);

        Assert.Equal(110, new FragileDecorator(basePrice).CalculatePrice(Weight), 2);
        Assert.Equal(120, new InsuranceDecorator(basePrice, 1000).CalculatePrice(Weight), 2); // 2% of 1000
        Assert.Equal(103, new InsuranceDecorator(basePrice, 50).CalculatePrice(Weight), 2);   // minimum 3 lei
    }

    [Fact]
    public void Decorators_CanWrapAnyStrategy_AndBeRepeated()
    {
        IPricingStrategy international = new InternationalPricing();
        IPricingStrategy doubleFragile = new FragileDecorator(new FragileDecorator(international));

        Assert.Equal(international.CalculatePrice(Weight) + 2 * FragileDecorator.Fee,
                     doubleFragile.CalculatePrice(Weight), 2);
    }

    [Fact]
    public void Options_WrapTheStrategyWithTheirDecorator()
    {
        IPricingStrategy basePrice = new FixedPricing(100);

        Assert.IsType<InsuranceDecorator>(new InsuranceOption(1000).Apply(basePrice));
        Assert.IsType<FragileDecorator>(new FragileOption().Apply(basePrice));
    }
}
