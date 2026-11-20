namespace Delivery.Notifications;

// Keeps the messages in memory. Useful for checks and tests.
public class InMemoryNotifier : INotifier
{
    public List<string> Sent { get; } = new();

    public void Send(string recipient, string message)
    {
        Sent.Add($"{recipient}: {message}");
    }
}
