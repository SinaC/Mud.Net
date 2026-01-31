using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class RoomGameActionInfo : ActorGameActionInfo, IRoomGameActionInfo
{
    public RoomGameActionInfo(Type commandExecutionType, RoomCommandAttribute itemCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, itemCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
