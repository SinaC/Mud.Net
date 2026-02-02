using Mud.Server.Ability.Interfaces;

namespace Mud.Server.Interfaces.GameAction;

public interface ISkillGameActionInfo : ICharacterGameActionInfo
{
    IAbilityDefinition AbilityDefinition { get; }
}
