namespace Mud.POC.Abilities2
{
    public interface IGameAction
    {
        // Guards the action against incorrect usage
        // Returns null if all guard pass
        // Returns error message describing failure
        string Guards(ActionInput actionInput);
        // Execute the action, Guards must be called before
        void Execute(ActionInput actionInput);
    }
}
