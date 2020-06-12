using Mud.Server.Input;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability
{
    public interface ISkillActionInput
    {
        ICharacter User { get; }
        string RawParameters { get; }
        CommandParameter[] Parameters { get; }
        IAbilityInfo AbilityInfo { get; }
    }
}
