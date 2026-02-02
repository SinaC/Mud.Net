using Mud.Server.Ability.Interfaces;

namespace Mud.Server.Ability.Skill.Interfaces;

public interface ISkill : IAbility
{
    // Guards the action against incorrect usage
    // Returns null if all guard pass
    // Returns error message describing failure
    string? Setup(ISkillActionInput skillActionInput);
    // Execute the action, Guards must be called before
    void Execute();
}
