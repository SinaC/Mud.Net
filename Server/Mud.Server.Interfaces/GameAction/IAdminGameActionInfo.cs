using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Interfaces.GameAction;

public interface IAdminGameActionInfo : IPlayerGameActionInfo
{
    IAdminGuard[] AdminGuards { get; }
}
