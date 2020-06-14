using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public interface ISkillActionInput
    {
        ICharacter User { get; }
        string RawParameters { get; }
        CommandParameter[] Parameters { get; }
        IAbilityInfo AbilityInfo { get; }
    }
}
