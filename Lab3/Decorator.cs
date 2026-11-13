using DeliverySOLID;

namespace DeliveryStructural;

// Base decorator for pricing: implements the same interface as the wrapped
// object and delegates the calculation to it. Each subclass adds one extra cost.
public abstract class PricingDecorator : IPricingStrategy
{
    protected readonly IPricingStrategy inner;

    protected PricingDecorator(IPricingStrategy inner)
    {
        this.inner = inner;
    }

    public virtual double CalculatePrice(double weightKg) => inner.CalculatePrice(weightKg);
}

// Insurance: 2% of the declared value, but at least 3 lei.
public class InsuranceDecorator : PricingDecorator
{
    private const double Rate = 0.02;
    private const double MinFee = 3.0;

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

// Special handling for fragile parcels: a fixed 10 lei fee.
public class FragileDecorator : PricingDecorator
{
    private const double Fee = 10.0;

    public FragileDecorator(IPricingStrategy inner) : base(inner)
    {
    }

    public override double CalculatePrice(double weightKg) =>
        Math.Round(base.CalculatePrice(weightKg) + Fee, 2);
}
