namespace Delivery.Pricing;

// An extra option the client can choose (insurance, fragile, ...).
// Each option knows how to wrap a pricing strategy with its decorator.
public interface IPricingOption
{
    string Label { get; }

    IPricingStrategy Apply(IPricingStrategy inner);
}
