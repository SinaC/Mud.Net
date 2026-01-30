using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class PlayerGameActionInfo : ActorGameActionInfo, IPlayerGameActionInfo
{
    public IPlayerGuard[] PlayerGuards { get; }

    public PlayerGameActionInfo(Type commandExecutionType, PlayerCommandAttribute playerCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IEnumerable<IActorGuard> actorGuards, IEnumerable<IPlayerGuard> guards)
        : base(commandExecutionType, playerCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, actorGuards)
    {
        PlayerGuards = guards.ToArray();
    }
}
