namespace DeliverySOLID;

public interface IPricingStrategy
{
    double CalculatePrice(double weightKg);
}

public class StandardPricing : IPricingStrategy
{
    public double CalculatePrice(double weightKg) => Math.Round(5.0 + 1.5 * weightKg, 2);
}

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
