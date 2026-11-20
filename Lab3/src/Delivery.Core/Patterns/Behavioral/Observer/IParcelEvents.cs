namespace Delivery.Behavioral.Observer;

// OBSERVER - Subject, as seen by subscribers: they can subscribe and unsubscribe at runtime.
public interface IParcelEvents
{
    void Subscribe(IParcelObserver observer);

    void Unsubscribe(IParcelObserver observer);
}

// Subject, as seen by the code that produces events.
public interface IParcelEventPublisher
{
    void Publish(ParcelEvent e);
}
