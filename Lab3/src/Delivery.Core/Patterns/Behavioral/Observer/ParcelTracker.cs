namespace Delivery.Behavioral.Observer;

// OBSERVER - ConcreteSubject. It knows its observers only through IParcelObserver.
public class ParcelTracker : IParcelEvents, IParcelEventPublisher
{
    private readonly List<IParcelObserver> observers = new();

    public int ObserverCount => observers.Count;

    public void Subscribe(IParcelObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void Unsubscribe(IParcelObserver observer) => observers.Remove(observer);

    public void Publish(ParcelEvent e)
    {
        // A copy, so an observer may unsubscribe while it is being notified.
        foreach (IParcelObserver observer in observers.ToArray())
        {
            observer.OnParcelEvent(e);
        }
    }
}
