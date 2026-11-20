namespace Delivery.Pricing;

// Standard delivery: 5 lei base fee + 1.50 lei per kg.
public class StandardPricing : IPricingStrategy
{
    public double CalculatePrice(double weightKg) => Math.Round(5.0 + 1.5 * weightKg, 2);
}
