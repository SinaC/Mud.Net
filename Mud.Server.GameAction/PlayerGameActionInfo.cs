using Mud.Server.Interfaces.GameAction;
using System;

namespace Mud.Server.GameAction
{
    public class PlayerGameActionInfo : GameActionInfo, IPlayerGameActionInfo
    {
        public bool MustBeImpersonated { get; }

        public bool CannotBeImpersonated { get; }

        public PlayerGameActionInfo(Type commandExecutionType, PlayerCommandAttribute playerCommandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandExecutionType, playerCommandAttribute, syntaxAttribute)
        {
            MustBeImpersonated = playerCommandAttribute.MustBeImpersonated;
            CannotBeImpersonated = playerCommandAttribute.CannotBeImpersonated;
        }
    }
}
