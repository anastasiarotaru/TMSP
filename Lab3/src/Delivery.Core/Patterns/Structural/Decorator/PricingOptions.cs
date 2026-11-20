using Delivery.Pricing;

namespace Delivery.Structural.Decorator;

// The options the client can choose. Each one wraps the strategy with its decorator,
// so the facade never needs to know which decorators exist.
public class InsuranceOption : IPricingOption
{
    private readonly double declaredValue;

    public InsuranceOption(double declaredValue)
    {
        this.declaredValue = declaredValue;
    }

    public string Label => "insurance";

    public IPricingStrategy Apply(IPricingStrategy inner) => new InsuranceDecorator(inner, declaredValue);
}

public class FragileOption : IPricingOption
{
    public string Label => "fragile";

    public IPricingStrategy Apply(IPricingStrategy inner) => new FragileDecorator(inner);
}
