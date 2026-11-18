using Delivery.Domain;
using Delivery.Pricing;

namespace Delivery.Creational;

// BUILDER: builds a Parcel step by step, with fluent methods.
// The checks are done in Build(), before the parcel is created.
public class ParcelBuilder
{
    private const double MaxWeightKg = 30.0;

    private string recipientName = "";
    private string recipientEmail = "";
    private Address? destination;
    private double weightKg;
    private string service = "standard";

    public ParcelBuilder To(string name, string email)
    {
        recipientName = name;
        recipientEmail = email;
        return this;
    }

    public ParcelBuilder Destination(Address address)
    {
        destination = address;
        return this;
    }

    public ParcelBuilder Weight(double kg)
    {
        weightKg = kg;
        return this;
    }

    public ParcelBuilder Service(string name)
    {
        service = name;
        return this;
    }

    // Validates the data, gets a code from the Singleton and calculates the price with the given strategy.
    public Parcel Build(IPricingStrategy pricing)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
        {
            throw new ArgumentException("The recipient name is missing.");
        }

        if (destination is null)
        {
            throw new ArgumentException("The delivery address is missing.");
        }

        if (weightKg <= 0 || weightKg > MaxWeightKg)
        {
            throw new ArgumentException($"The weight must be between 0 and {MaxWeightKg} kg");
        }

        string trackingId = TrackingIdGenerator.Instance.Next();
        double price = pricing.CalculatePrice(weightKg);

        return new Parcel(trackingId, recipientName, recipientEmail,
                          destination, weightKg, service, price);
    }
}
