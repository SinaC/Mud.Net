using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class ItemGameActionInfo : ActorGameActionInfo, IItemGameActionInfo
{
    public ItemGameActionInfo(Type commandExecutionType, ItemCommandAttribute itemCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, itemCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
