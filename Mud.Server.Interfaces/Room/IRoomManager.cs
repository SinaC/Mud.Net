using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using System;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Room
{
    public interface IRoomManager
    {
        IEnumerable<IRoom> Rooms { get; }

        IRoom GetRandomRoom(ICharacter character);

        IRoom DefaultRecallRoom { get; }
        IRoom DefaultDeathRoom { get; }
        IRoom MudSchoolRoom { get; }

        IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area);

        void RemoveRoom(IRoom room);
    }
}
