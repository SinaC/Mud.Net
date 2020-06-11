namespace Mud.POC.Abilities2
{
    public interface ISkill : IAbility
    {
        // Guards the action against incorrect usage
        // Returns null if all guard pass
        // Returns error message describing failure
        string Setup(SkillActionInput skillActionInput);
        // Execute the action, Guards must be called before
        void Execute();
    }
}
