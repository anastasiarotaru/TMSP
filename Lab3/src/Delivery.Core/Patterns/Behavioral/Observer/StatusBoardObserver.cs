using Delivery.Domain;

namespace Delivery.Behavioral.Observer;

// A live board: how many parcels are in each status right now.
public class StatusBoardObserver : IParcelObserver
{
    private readonly Dictionary<string, ParcelStatus> current = new();

    public void OnParcelEvent(ParcelEvent e) => current[e.Parcel.TrackingId] = e.Parcel.Status;

    public int CountIn(ParcelStatus status) => current.Values.Count(s => s == status);

    public IReadOnlyDictionary<ParcelStatus, int> Counts =>
        Enum.GetValues<ParcelStatus>().ToDictionary(s => s, CountIn);
}
