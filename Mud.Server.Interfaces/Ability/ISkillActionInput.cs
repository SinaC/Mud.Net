using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Ability;

public interface ISkillActionInput
{
    ICharacter User { get; }
    ICommandParameter[] Parameters { get; }
    IAbilityDefinition AbilityDefinition { get; }
}
