using Delivery.Behavioral.Observer;

namespace Delivery.Tests.Stubs;

// An observer that only remembers the events it received.
public class RecordingObserver : IParcelObserver
{
    public List<ParcelEvent> Received { get; } = new();

    public void OnParcelEvent(ParcelEvent e) => Received.Add(e);
}
