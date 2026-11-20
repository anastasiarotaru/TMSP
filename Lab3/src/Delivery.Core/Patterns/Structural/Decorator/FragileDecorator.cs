using Delivery.Pricing;

namespace Delivery.Structural.Decorator;

// Special handling for fragile parcels: a fixed 10 lei fee.
public class FragileDecorator : PricingDecorator
{
    public const double Fee = 10.0;

    public FragileDecorator(IPricingStrategy inner) : base(inner)
    {
    }

    public override double CalculatePrice(double weightKg) =>
        Math.Round(base.CalculatePrice(weightKg) + Fee, 2);
}
