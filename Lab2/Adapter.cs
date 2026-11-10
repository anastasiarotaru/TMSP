using DeliverySOLID;

namespace DeliveryStructural;

// API-ul unui curier partener international (cod extern, nu il putem modifica).
// Are o interfata incompatibila cu IPricingStrategy: lucreaza in grame,
// primeste un cod de zona si intoarce pretul in eurocenti.
public class PartnerCourierApi
{
    public int QuoteInEuroCents(int grams, string zoneCode)
    {
        int baseCents = zoneCode == "EU" ? 900 : 1500;
        int perHundredGrams = zoneCode == "EU" ? 12 : 20;
        return baseCents + (grams + 99) / 100 * perHundredGrams;
    }
}

// Adapter: face ca API-ul partenerului sa arate ca o strategie de pret obisnuita.
// DeliveryService nu stie ca in spate se afla un serviciu extern.
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
