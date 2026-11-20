namespace Delivery.Behavioral.Command;

// COMMAND - Invoker. Runs commands, keeps the history and undoes them in reverse order.
// It knows only ICommand, never the concrete commands.
public class CommandInvoker
{
    private readonly Stack<ICommand> done = new();
    private readonly List<string> history = new();

    public bool CanUndo => done.Count > 0;

    public IReadOnlyList<string> History => history;

    public void Execute(ICommand command)
    {
        command.Execute();          // if it throws, it is not added to the history
        done.Push(command);
        history.Add($"done: {command.Description}");
    }

    public bool Undo()
    {
        if (!done.TryPop(out ICommand? command))
        {
            return false;
        }

        command.Undo();
        history.Add($"undone: {command.Description}");
        return true;
    }
}
