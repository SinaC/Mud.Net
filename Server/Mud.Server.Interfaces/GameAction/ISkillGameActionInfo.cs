using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Interfaces.GameAction;

public interface ISkillGameActionInfo : ICharacterGameActionInfo
{
    IAbilityDefinition AbilityDefinition { get; }
}
