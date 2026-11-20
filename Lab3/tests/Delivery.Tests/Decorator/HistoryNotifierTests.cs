using Delivery.Notifications;
using Delivery.Structural.Decorator;

namespace Delivery.Tests.Decorator;

public class HistoryNotifierTests
{
    [Fact]
    public void HistoryNotifier_RecordsTheMessage_AndPassesItOnUnchanged()
    {
        var inner = new InMemoryNotifier();
        var notifier = new HistoryNotifier(inner, () => new DateTime(2026, 1, 1, 9, 30, 0));

        notifier.Send("ana@example.com", "hello");

        Assert.Equal(new[] { "[09:30:00] ana@example.com: hello" }, notifier.History);
        Assert.Equal(new[] { "ana@example.com: hello" }, inner.Sent);
    }
}
