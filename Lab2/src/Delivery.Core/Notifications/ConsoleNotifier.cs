namespace Delivery.Notifications;

// Prints the messages to the console.
public class ConsoleNotifier : INotifier
{
    public void Send(string recipient, string message)
    {
        Console.WriteLine($"  [notification -> {recipient}] {message}");
    }
}
