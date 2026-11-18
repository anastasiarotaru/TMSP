using Delivery.Pricing;

namespace Delivery.Structural.Decorator;

// DECORATOR (base): implements the same interface as the wrapped object
// and delegates the calculation to it. Each subclass adds one extra cost.
public abstract class PricingDecorator : IPricingStrategy
{
    protected readonly IPricingStrategy inner;

    protected PricingDecorator(IPricingStrategy inner)
    {
        this.inner = inner;
    }

    public virtual double CalculatePrice(double weightKg) => inner.CalculatePrice(weightKg);
}
