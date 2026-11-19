using Delivery.Pricing;

namespace Delivery.Tests.Stubs;

// A pricing strategy with a known price, so decorator fees are easy to check.
public class FixedPricing : IPricingStrategy
{
    private readonly double price;

    public FixedPricing(double price) => this.price = price;

    public double CalculatePrice(double weightKg) => price;
}
