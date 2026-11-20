using Delivery.Pricing;

namespace Delivery.Creational;

// FACTORY METHOD: the base class declares the creation method.
// Each subclass decides which pricing strategy is created and under which service name.
public abstract class PricingFactory
{
    public abstract string ServiceName { get; }

    public abstract IPricingStrategy CreatePricing();
}

public class StandardPricingFactory : PricingFactory
{
    public override string ServiceName => "standard";

    public override IPricingStrategy CreatePricing() => new StandardPricing();
}

public class ExpressPricingFactory : PricingFactory
{
    public override string ServiceName => "express";

    public override IPricingStrategy CreatePricing() =>
        new ExpressPricing(new StandardPricing());
}

public class OvernightPricingFactory : PricingFactory
{
    public override string ServiceName => "overnight";

    public override IPricingStrategy CreatePricing() =>
        new OvernightPricing(new StandardPricing());
}

public class InternationalPricingFactory : PricingFactory
{
    public override string ServiceName => "international";

    public override IPricingStrategy CreatePricing() => new InternationalPricing();
}
