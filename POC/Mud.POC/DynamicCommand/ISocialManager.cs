using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.DynamicCommand
{
    public interface ISocialManager
    {
        IEnumerable<IGameActionInfo> GetGameActions();
    }
}