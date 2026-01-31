namespace Mud.Server.Interfaces.GameAction;

public interface IGameAction
{
    // Guards the action against incorrect usage
    // Returns null if all guards pass
    // Returns error message describing failure
    string? CanExecute(IActionInput actionInput);
    // Execute the action, Guards must be called before
    void Execute(IActionInput actionInput);
    // Build the command syntax for help or error messages
    string BuildCommandSyntax();
}
