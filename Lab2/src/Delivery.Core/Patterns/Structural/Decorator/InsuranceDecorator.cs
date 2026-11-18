using Delivery.Pricing;

namespace Delivery.Structural.Decorator;

// Insurance: 2% of the declared value, but at least 3 lei.
public class InsuranceDecorator : PricingDecorator
{
    public const double Rate = 0.02;
    public const double MinFee = 3.0;

    private readonly double declaredValue;

    public InsuranceDecorator(IPricingStrategy inner, double declaredValue) : base(inner)
    {
        if (declaredValue <= 0)
        {
            throw new ArgumentException("The declared value must be positive");
        }

        this.declaredValue = declaredValue;
    }

    public override double CalculatePrice(double weightKg) =>
        Math.Round(base.CalculatePrice(weightKg) + Math.Max(MinFee, declaredValue * Rate), 2);
}
