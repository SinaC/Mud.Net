using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.Abilities2
{
    public interface ISkillActionInput
    {
        ICharacter User { get; }
        string RawParameters { get; }
        ICommandParameter[] Parameters { get; }
        IAbilityInfo AbilityInfo { get; }
    }
}
