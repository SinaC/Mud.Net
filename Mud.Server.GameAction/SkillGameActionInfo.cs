using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class SkillGameActionInfo : CharacterGameActionInfo, ISkillGameActionInfo
{
    public IAbilityDefinition AbilityDefinition { get; }

    public SkillGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IAbilityDefinition abilityDefinition, IEnumerable<ICharacterGuard> guards)
        : base(commandExecutionType, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, guards)
    {
        AbilityDefinition = abilityDefinition;
    }
}
