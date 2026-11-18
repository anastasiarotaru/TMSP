using Delivery.Domain;

namespace Delivery.Services;

// Prints the shipping label to the console. Changes only if the label layout changes.
public class LabelPrinter : ILabelPrinter
{
    public void PrintLabel(Parcel parcel)
    {
        Console.WriteLine("============================================");
        Console.WriteLine($"AWB:        {parcel.TrackingId}");
        Console.WriteLine($"Recipient:  {parcel.RecipientName}");
        Console.WriteLine($"Address:    {parcel.Destination}");
        Console.WriteLine($"Weight:     {parcel.WeightKg} kg");
        Console.WriteLine($"Service:    {parcel.Service}");
        Console.WriteLine($"Price:      {parcel.Price:F2} lei");
        Console.WriteLine($"Status:     {parcel.Status}");
        Console.WriteLine("============================================");
    }
}
