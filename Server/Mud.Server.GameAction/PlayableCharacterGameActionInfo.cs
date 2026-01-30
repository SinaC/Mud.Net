using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class PlayableCharacterGameActionInfo : CharacterGameActionInfo, IPlayableCharacterGameActionInfo
{
    public PlayableCharacterGameActionInfo(Type commandExecutionType, PlayableCharacterCommandAttribute playableCharacterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IEnumerable<IActorGuard> actorGuards, IEnumerable<ICharacterGuard> characterGuards)
        : base(commandExecutionType, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, actorGuards, characterGuards)
    {
    }
}
