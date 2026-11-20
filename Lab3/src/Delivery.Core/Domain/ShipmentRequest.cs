using Delivery.Pricing;

namespace Delivery.Domain;

// Everything the client says about a shipment. The extra options are objects,
// so a new option can be added without changing this record.
public record ShipmentRequest(
    string RecipientName,
    string RecipientEmail,
    Address Destination,
    double WeightKg,
    string Service,
    IReadOnlyList<IPricingOption> Options);
