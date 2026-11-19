using Delivery.Domain;
using Delivery.Pricing;
using Delivery.Services;

namespace Delivery.Tests.Stubs;

// Stubs for the facade's subsystem. All of them write into one shared call log,
// so a test can check the order in which the facade calls them.
public class CallLog
{
    public List<string> Calls { get; } = new();
}

public class StubDeliveryService : IDeliveryService
{
    private readonly CallLog log;
    private readonly Dictionary<string, Parcel> parcels = new();

    public StubDeliveryService(CallLog log) => this.log = log;

    public IPricingStrategy? LastPricing { get; private set; }

    public Parcel Register(string recipientName, string recipientEmail, Address destination,
                           double weightKg, string serviceLabel, IPricingStrategy pricing)
    {
        log.Calls.Add($"Register:{serviceLabel}");
        LastPricing = pricing;
        var parcel = new Parcel("TEST01", recipientName, recipientEmail, destination,
                                weightKg, serviceLabel, pricing.CalculatePrice(weightKg));
        parcels[parcel.TrackingId] = parcel;
        return parcel;
    }

    public void Advance(string trackingId, ParcelStatus newStatus)
    {
        log.Calls.Add($"Advance:{newStatus}");
        parcels[trackingId].Status = newStatus;
    }

    public Parcel Get(string trackingId)
    {
        log.Calls.Add("Get");
        return parcels[trackingId];
    }
}

public class StubLabelPrinter : ILabelPrinter
{
    private readonly CallLog log;

    public StubLabelPrinter(CallLog log) => this.log = log;

    public void PrintLabel(Parcel parcel) => log.Calls.Add($"PrintLabel:{parcel.TrackingId}");
}
