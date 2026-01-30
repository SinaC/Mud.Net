using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class ItemGameActionInfo : ActorGameActionInfo, IItemGameActionInfo
{
    public ItemGameActionInfo(Type commandExecutionType, ItemCommandAttribute itemCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IEnumerable<IActorGuard> actorGuards)
        : base(commandExecutionType, itemCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, actorGuards)
    {
    }
}
