namespace DeliverySOLID;

// Abstraction: every pricing rule implements this interface.
public interface IPricingStrategy
{
    double CalculatePrice(double weightKg);
}

// Standard delivery: 5 lei base fee + 1.50 lei per kg.
public class StandardPricing : IPricingStrategy
{
    public double CalculatePrice(double weightKg) => Math.Round(5.0 + 1.5 * weightKg, 2);
}

// Express delivery: the standard price doubled.
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

// Next-day delivery: the base price plus a fixed 15 lei surcharge.
public class OvernightPricing : IPricingStrategy
{
    private readonly IPricingStrategy baseStrategy;

    public OvernightPricing(IPricingStrategy baseStrategy)
    {
        this.baseStrategy = baseStrategy;
    }

    public double CalculatePrice(double weightKg) =>
        Math.Round(baseStrategy.CalculatePrice(weightKg) + 15.0, 2);
}
