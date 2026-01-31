using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class SkillGameActionInfo : CharacterGameActionInfo, ISkillGameActionInfo
{
    public IAbilityDefinition AbilityDefinition { get; }

    public SkillGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IAbilityDefinition abilityDefinition)
        : base(commandExecutionType, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
        AbilityDefinition = abilityDefinition;
    }
}
