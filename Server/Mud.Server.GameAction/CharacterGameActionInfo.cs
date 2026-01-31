using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class CharacterGameActionInfo : ActorGameActionInfo, ICharacterGameActionInfo
{
    public CharacterGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
