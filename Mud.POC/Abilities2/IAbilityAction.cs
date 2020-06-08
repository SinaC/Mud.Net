namespace Mud.POC.Abilities2
{
    public interface IAbilityAction
    {
        // Guards the action against incorrect usage
        // Returns null if all guard pass
        // Returns error message describing failure
        string Setup(AbilityActionInput actionInput);
        // Execute the action, Guards must be called before
        void Execute(AbilityActionInput actionInput);
    }
}
