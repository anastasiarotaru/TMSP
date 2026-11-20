namespace Delivery.Pricing;

// International delivery (EU): 9 EUR + 0.12 EUR for every started 100 g,
// converted to lei.
public class InternationalPricing : IPricingStrategy
{
    private const double LeiPerEuro = 19.5;
    private const double BaseEuro = 9.0;
    private const double EuroPerHundredGrams = 0.12;

    public double CalculatePrice(double weightKg)
    {
        int grams = (int)Math.Ceiling(weightKg * 1000);
        int startedHundreds = (grams + 99) / 100;
        double euro = BaseEuro + startedHundreds * EuroPerHundredGrams;
        return Math.Round(euro * LeiPerEuro, 2);
    }
}
