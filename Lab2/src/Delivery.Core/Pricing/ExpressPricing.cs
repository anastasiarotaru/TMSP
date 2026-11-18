namespace Delivery.Pricing;

// Express delivery: the base price doubled.
public class ExpressPricing : IPricingStrategy
{
    private readonly IPricingStrategy baseStrategy;

    public ExpressPricing(IPricingStrategy baseStrategy)
    {
        this.baseStrategy = baseStrategy;
    }

    public double CalculatePrice(double weightKg) =>
        Math.Round(baseStrategy.CalculatePrice(weightKg) * 2.0, 2);
}
