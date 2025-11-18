using Mud.Server.Common;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class PlayableCharacterGameActionInfo : CharacterGameActionInfo, IPlayableCharacterGameActionInfo
{
    public PlayableCharacterGameActionInfo(Type commandExecutionType, PlayableCharacterCommandAttribute playableCharacterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
