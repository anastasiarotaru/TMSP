namespace DeliverySOLID;

// FACTORY METHOD: the base class declares the creation method.
// Each subclass decides which pricing strategy is created.
public abstract class PricingFactory
{
    public abstract IPricingStrategy CreatePricing();
}

public class StandardPricingFactory : PricingFactory
{
    public override IPricingStrategy CreatePricing() => new StandardPricing();
}

public class ExpressPricingFactory : PricingFactory
{
    public override IPricingStrategy CreatePricing() =>
        new ExpressPricing(new StandardPricing());
}

public class OvernightPricingFactory : PricingFactory
{
    public override IPricingStrategy CreatePricing() =>
        new OvernightPricing(new StandardPricing());
}
