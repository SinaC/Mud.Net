using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class NonPlayableCharacterGameActionInfo : CharacterGameActionInfo, INonPlayableCharacterGameActionInfo
{
    public NonPlayableCharacterGameActionInfo(Type commandExecutionType, NonPlayableCharacterCommandAttribute nonPlayableCharacterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, nonPlayableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
