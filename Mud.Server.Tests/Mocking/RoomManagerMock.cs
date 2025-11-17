namespace Mud.Server.Tests.Mocking
{
    /*
    internal class RoomManagerMock : IRoomManager
    {
        private readonly List<RoomBlueprint> _roomBlueprints = new List<RoomBlueprint>();

        private readonly List<IRoom> _rooms = new List<IRoom>();

        public IEnumerable<IRoom> Rooms => _rooms;

        public IRoom DefaultRecallRoom => new Mock<IRoom>().Object;

        public IRoom DefaultDeathRoom => new Mock<IRoom>().Object;

        public IRoom MudSchoolRoom => new Mock<IRoom>().Object;

        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints;

        public IRoom NullRoom => new Mock<IRoom>().Object;

        public IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction)
        {
            throw new NotImplementedException();
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            IRoom room = new Room.Room(guid, blueprint, area);
            _rooms.Add(room);
            return room;
        }

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            _roomBlueprints.Add(blueprint);
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

        public void Clear()
        {
            _rooms.Clear();
            _roomBlueprints.Clear();
        }

        public void Cleanup()
        {
        }
    }
    */
}
