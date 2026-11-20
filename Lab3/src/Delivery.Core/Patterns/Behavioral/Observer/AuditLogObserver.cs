namespace Delivery.Behavioral.Observer;

// The dispatcher's log: every event, with time and type, including undo corrections.
public class AuditLogObserver : IParcelObserver
{
    private readonly List<string> entries = new();
    private readonly Func<DateTime> clock;

    public AuditLogObserver(Func<DateTime>? clock = null)
    {
        this.clock = clock ?? (() => DateTime.Now);
    }

    public IReadOnlyList<string> Entries => entries;

    public void OnParcelEvent(ParcelEvent e) =>
        entries.Add($"[{clock():HH:mm:ss}] {e.Parcel.TrackingId} {e.Kind,-18} {e.Description}");
}
