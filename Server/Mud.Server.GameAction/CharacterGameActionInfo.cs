using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class CharacterGameActionInfo : ActorGameActionInfo, ICharacterGameActionInfo
{
    public ICharacterGuard[] CharacterGuards { get; }

    public CharacterGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IEnumerable<IActorGuard> actorGuards, IEnumerable<ICharacterGuard> guards)
        : base(commandExecutionType, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, actorGuards)
    {
        CharacterGuards = guards.ToArray();
    }
}
