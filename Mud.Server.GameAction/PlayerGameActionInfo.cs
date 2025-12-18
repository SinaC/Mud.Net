using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class PlayerGameActionInfo : ActorGameActionInfo, IPlayerGameActionInfo
{
    public bool MustBeImpersonated { get; }

    public bool CannotBeImpersonated { get; }

    public PlayerGameActionInfo(Type commandExecutionType, PlayerCommandAttribute playerCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, playerCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
        MustBeImpersonated = playerCommandAttribute.MustBeImpersonated;
        CannotBeImpersonated = playerCommandAttribute.CannotBeImpersonated;
    }
}
