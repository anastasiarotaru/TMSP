using Delivery.Notifications;

namespace Delivery.Behavioral.Observer;

// Sends every event to the customer through INotifier
// (in the app: HistoryNotifier decorator -> Serilog adapter, from Laboratory 2).
public class CustomerNotificationObserver : IParcelObserver
{
    private readonly INotifier notifier;

    public CustomerNotificationObserver(INotifier notifier)
    {
        this.notifier = notifier;
    }

    public void OnParcelEvent(ParcelEvent e) => notifier.Send(e.Parcel.RecipientEmail, e.Description);
}
