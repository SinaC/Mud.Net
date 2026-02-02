using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class PlayerGameActionInfo : ActorGameActionInfo, IPlayerGameActionInfo
{
    public PlayerGameActionInfo(Type commandExecutionType, PlayerCommandAttribute playerCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, playerCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
