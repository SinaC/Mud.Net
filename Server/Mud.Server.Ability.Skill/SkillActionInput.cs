using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Ability.Skill;

public class SkillActionInput : ISkillActionInput
{
    public ICharacter User { get; }
    public ICommandParameter[] Parameters { get; }
    public IAbilityDefinition AbilityDefinition { get; }

    public SkillActionInput(IActionInput actionInput, IAbilityDefinition abilityDefinition, ICharacter user)
    {
        User = user;
        Parameters = actionInput.Parameters;
        AbilityDefinition = abilityDefinition;
    }

    public SkillActionInput(IAbilityDefinition abilityDefinition, ICharacter user, params ICommandParameter[] parameters)
    {
        User = user;
        Parameters = parameters;
        AbilityDefinition = abilityDefinition;
    }
}
