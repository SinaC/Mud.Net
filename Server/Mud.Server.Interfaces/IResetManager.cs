using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces
{
    public interface IResetManager
    {
        void ResetArea(IArea area);
        void ResetRoom(IRoom room);
    }
}
