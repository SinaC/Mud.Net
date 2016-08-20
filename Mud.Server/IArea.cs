using System.Collections.Generic;

namespace Mud.Server
{
    public interface IArea
    {
        string DisplayName { get; }
        int MinLevel { get; }
        int MaxLevel { get; }
        string Builders { get; }
        string Credits { get; }

        IEnumerable<IRoom> Rooms { get; }
        IEnumerable<IPlayer> Players { get; }

        bool AddRoom(IRoom room);
        bool RemoveRoom(IRoom room);
    }
}
