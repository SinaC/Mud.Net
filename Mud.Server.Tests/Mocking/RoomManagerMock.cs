using Moq;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Tests.Mocking
{
    public class RoomManagerMock : IRoomManager
    {
        private readonly List<RoomBlueprint> _roomBlueprints;

        private readonly List<IRoom> _rooms;

        public IEnumerable<IRoom> Rooms => _rooms;

        public IRoom DefaultRecallRoom => new Mock<IRoom>().Object;

        public IRoom DefaultDeathRoom => new Mock<IRoom>().Object;

        public IRoom MudSchoolRoom => new Mock<IRoom>().Object;

        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints;

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            IRoom room = new Room.Room(guid, blueprint, area);
            _rooms.Add(room);
            return room;
        }

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public IRoom GetRandomRoom(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public RoomBlueprint GetRoomBlueprint(int id) => _roomBlueprints.FirstOrDefault(x => x.Id == id);

        public void RemoveRoom(IRoom room)
        {
            throw new NotImplementedException();
        }
    }
}
