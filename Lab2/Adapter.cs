using DeliverySOLID;

namespace DeliveryStructural;

// The API of an international partner courier (external code we cannot modify).
// Its interface is incompatible with IPricingStrategy: it works in grams,
// takes a zone code and returns the price in euro cents.
public class PartnerCourierApi
{
    public int QuoteInEuroCents(int grams, string zoneCode)
    {
        int baseCents = zoneCode == "EU" ? 900 : 1500;
        int perHundredGrams = zoneCode == "EU" ? 12 : 20;
        return baseCents + (grams + 99) / 100 * perHundredGrams;
    }
}

// Adapter: makes the partner's API look like an ordinary pricing strategy.
// DeliveryService does not know that an external service is behind it.
public class PartnerPricingAdapter : IPricingStrategy
{
    private const double LeiPerEuro = 19.5;

    private readonly PartnerCourierApi api;
    private readonly string zoneCode;

    public PartnerPricingAdapter(PartnerCourierApi api, string zoneCode)
    {
        this.api = api;
        this.zoneCode = zoneCode;
    }

    public double CalculatePrice(double weightKg)
    {
        int grams = (int)Math.Ceiling(weightKg * 1000);
        int cents = api.QuoteInEuroCents(grams, zoneCode);
        return Math.Round(cents / 100.0 * LeiPerEuro, 2);
    }
}

// Factory Method (from Laboratory 1): the factory for the international service.
// It creates the adapter, so the new service is added like any other.
public class InternationalPricingFactory : PricingFactory
{
    private readonly string zoneCode;

    public InternationalPricingFactory(string zoneCode)
    {
        this.zoneCode = zoneCode;
    }

    public override IPricingStrategy CreatePricing() =>
        new PartnerPricingAdapter(new PartnerCourierApi(), zoneCode);
}
