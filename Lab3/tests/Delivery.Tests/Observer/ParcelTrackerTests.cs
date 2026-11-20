using Delivery.Behavioral.Observer;
using Delivery.Domain;
using Delivery.Notifications;
using Delivery.Tests.Stubs;

namespace Delivery.Tests.Observer;

public class ParcelTrackerTests
{
    private readonly ParcelTracker tracker = new();
    private readonly ParcelEvent sample = new(TestParcels.New(), ParcelEventKind.StatusChanged, "test");

    [Fact]
    public void AllSubscribedObservers_AreNotified()
    {
        var first = new RecordingObserver();
        var second = new RecordingObserver();
        tracker.Subscribe(first);
        tracker.Subscribe(second);

        tracker.Publish(sample);

        Assert.Same(sample, Assert.Single(first.Received));
        Assert.Same(sample, Assert.Single(second.Received));
    }

    [Fact]
    public void UnsubscribedObserver_IsNotNotified()
    {
        var stays = new RecordingObserver();
        var leaves = new RecordingObserver();
        tracker.Subscribe(stays);
        tracker.Subscribe(leaves);

        tracker.Unsubscribe(leaves);
        tracker.Publish(sample);

        Assert.Single(stays.Received);
        Assert.Empty(leaves.Received);
    }

    [Fact]
    public void SubscribingTwice_StillNotifiesOnce()
    {
        var observer = new RecordingObserver();
        tracker.Subscribe(observer);
        tracker.Subscribe(observer);

        tracker.Publish(sample);

        Assert.Single(observer.Received);
    }

    [Fact]
    public void ObserverMayUnsubscribeWhileBeingNotified()
    {
        var other = new RecordingObserver();
        var leaving = new SelfRemovingObserver(tracker);
        tracker.Subscribe(leaving);
        tracker.Subscribe(other);

        tracker.Publish(sample);
        tracker.Publish(sample);

        Assert.Equal(1, leaving.Calls);
        Assert.Equal(2, other.Received.Count);
    }

    [Fact]
    public void ReceiverChanges_ReachTheConcreteObservers()
    {
        var notifier = new InMemoryNotifier();
        var board = new StatusBoardObserver();
        tracker.Subscribe(new CustomerNotificationObserver(notifier));
        tracker.Subscribe(board);
        var delivery = new Delivery.Services.DeliveryService(tracker);

        Parcel parcel = TestParcels.Register(delivery);
        delivery.Advance(parcel.TrackingId);

        Assert.Equal(2, notifier.Sent.Count);
        Assert.StartsWith("ana@example.com:", notifier.Sent[1]);
        Assert.Equal(1, board.CountIn(ParcelStatus.PickedUp));
        Assert.Equal(0, board.CountIn(ParcelStatus.Registered));
    }

    private class SelfRemovingObserver : IParcelObserver
    {
        private readonly ParcelTracker tracker;

        public SelfRemovingObserver(ParcelTracker tracker) => this.tracker = tracker;

        public int Calls { get; private set; }

        public void OnParcelEvent(ParcelEvent e)
        {
            Calls++;
            tracker.Unsubscribe(this);
        }
    }
}
