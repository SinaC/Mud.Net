using Mud.Server.Ability.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Skill.Interfaces;

public interface ISkillActionInput
{
    ICharacter User { get; }
    ICommandParameter[] Parameters { get; }
    IAbilityDefinition AbilityDefinition { get; }
}
