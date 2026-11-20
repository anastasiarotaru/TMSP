namespace Delivery.Behavioral.Command;

// COMMAND - every dispatcher action is an object that can be executed and undone.
public interface ICommand
{
    string Description { get; }

    void Execute();

    void Undo();
}
