namespace Mud.POC.Abilities2
{
    public interface IGameAction<in TActionInput>
        where TActionInput : ActionInput
    {
        // Guards the action against incorrect usage
        // Returns null if all guard pass
        // Returns error message describing failure
        string Guards(TActionInput actionInput);
        // Execute the action, Guards must be called before
        void Execute(TActionInput actionInput);
    }
}
