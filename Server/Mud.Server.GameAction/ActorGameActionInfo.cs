using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class ActorGameActionInfo : GameActionInfo, IActorGameActionInfo
{
    public IActorGuard[] ActorGuards { get; }

    public ActorGameActionInfo(Type commandExecutionType, ActorCommandAttribute adminCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IEnumerable<IActorGuard> actorGuards)
        : base(commandExecutionType, adminCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
        ActorGuards = actorGuards.ToArray();
    }
}
