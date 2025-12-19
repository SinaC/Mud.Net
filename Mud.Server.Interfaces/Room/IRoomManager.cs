using Mud.Domain;
using Mud.Blueprints.Room;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Room;

public interface IRoomManager
{
    IReadOnlyCollection<RoomBlueprint> RoomBlueprints { get; }

    RoomBlueprint? GetRoomBlueprint(int id);

    void AddRoomBlueprint(RoomBlueprint blueprint);

    IEnumerable<IRoom> Rooms { get; }

    IRoom GetRandomRoom(ICharacter character);

    IRoom NullRoom { get; }
    IRoom DefaultRecallRoom { get; }
    IRoom DefaultDeathRoom { get; }
    IRoom MudSchoolRoom { get; }

    IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area);
    IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction);

    void RemoveRoom(IRoom room);

    void Cleanup();
}
