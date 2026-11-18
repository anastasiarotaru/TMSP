namespace Delivery.Notifications;

// Abstraction: how a message is sent to the customer (DIP).
public interface INotifier
{
    void Send(string recipient, string message);
}
