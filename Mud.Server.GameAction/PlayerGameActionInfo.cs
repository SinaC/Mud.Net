using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class PlayerGameActionInfo : GameActionInfo, IPlayerGameActionInfo
{
    public bool MustBeImpersonated { get; }

    public bool CannotBeImpersonated { get; }

    public PlayerGameActionInfo(Type commandExecutionType, PlayerCommandAttribute playerCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes)
        : base(commandExecutionType, playerCommandAttribute, syntaxAttribute, aliasAttributes)
    {
        MustBeImpersonated = playerCommandAttribute.MustBeImpersonated;
        CannotBeImpersonated = playerCommandAttribute.CannotBeImpersonated;
    }
}
