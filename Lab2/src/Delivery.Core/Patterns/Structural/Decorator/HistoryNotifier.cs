using Delivery.Notifications;

namespace Delivery.Structural.Decorator;

// Keeps a history with the time of each notification, then passes it on.
public class HistoryNotifier : NotifierDecorator
{
    private readonly List<string> history = new();
    private readonly Func<DateTime> clock;

    public IReadOnlyList<string> History => history;

    public HistoryNotifier(INotifier inner, Func<DateTime>? clock = null) : base(inner)
    {
        this.clock = clock ?? (() => DateTime.Now);
    }

    public override void Send(string recipient, string message)
    {
        history.Add($"[{clock():HH:mm:ss}] {recipient}: {message}");
        base.Send(recipient, message);
    }
}
