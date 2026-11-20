namespace Delivery.Pricing;

// Next-day delivery: the base price plus a fixed 15 lei surcharge.
public class OvernightPricing : IPricingStrategy
{
    private readonly IPricingStrategy baseStrategy;

    public OvernightPricing(IPricingStrategy baseStrategy)
    {
        this.baseStrategy = baseStrategy;
    }

    public double CalculatePrice(double weightKg) =>
        Math.Round(baseStrategy.CalculatePrice(weightKg) + 15.0, 2);
}
