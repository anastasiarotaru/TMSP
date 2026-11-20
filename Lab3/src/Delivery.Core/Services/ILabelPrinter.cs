using Delivery.Domain;

namespace Delivery.Services;

public interface ILabelPrinter
{
    void PrintLabel(Parcel parcel);
}
