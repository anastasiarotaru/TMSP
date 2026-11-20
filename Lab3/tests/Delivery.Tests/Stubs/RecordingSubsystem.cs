using Delivery.Behavioral.State;
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

    public Parcel Get(string trackingId)
    {
        log.Calls.Add("Get");
        return parcels[trackingId];
    }

    public void Advance(string trackingId)
    {
        log.Calls.Add("Advance");
        parcels[trackingId].Advance();
    }

    public void Cancel(string trackingId) => log.Calls.Add("Cancel");

    public void ChangeDestination(string trackingId, Address newDestination) => log.Calls.Add("ChangeDestination");

    public void RestoreState(string trackingId, IParcelState previous) => log.Calls.Add("RestoreState");
}

public class StubLabelPrinter : ILabelPrinter
{
    private readonly CallLog log;

    public StubLabelPrinter(CallLog log) => this.log = log;

    public void PrintLabel(Parcel parcel) => log.Calls.Add($"PrintLabel:{parcel.TrackingId}");
}
