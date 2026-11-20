using Delivery.Notifications;

namespace Delivery.Structural.Decorator;

// DECORATOR (base) for notifications.
public abstract class NotifierDecorator : INotifier
{
    protected readonly INotifier inner;

    protected NotifierDecorator(INotifier inner)
    {
        this.inner = inner;
    }

    public virtual void Send(string recipient, string message) => inner.Send(recipient, message);
}
