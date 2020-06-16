using Mud.Domain;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IAdminGameActionInfo : IPlayerGameActionInfo
    {
        AdminLevels MinLevel { get; }
    }
}
