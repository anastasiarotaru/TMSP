namespace Delivery.Behavioral.Observer;

// OBSERVER - the interface every subscriber implements.
public interface IParcelObserver
{
    void OnParcelEvent(ParcelEvent e);
}
