namespace Delivery.Pricing;

// Abstraction: every pricing rule implements this interface (OCP).
public interface IPricingStrategy
{
    double CalculatePrice(double weightKg);
}
