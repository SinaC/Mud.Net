using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class ActorGameActionInfo : GameActionInfo, IActorGameActionInfo
{
    public ActorGameActionInfo(Type commandExecutionType, ActorCommandAttribute adminCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, adminCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
    }
}
