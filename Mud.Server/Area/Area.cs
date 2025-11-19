using Mud.Server.Blueprints.Area;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Area;

public class Area : IArea
{
    private readonly List<IRoom> _rooms;

    public Area(Guid id, AreaBlueprint blueprint)
    {
        Id = id;

        Blueprint = blueprint;
        DisplayName = blueprint.Name;
        Builders = blueprint.Builders;
        Credits = blueprint.Credits;

        _rooms = [];
    }

    #region IArea

    public Guid Id { get; }

    public AreaBlueprint Blueprint { get; }

    public string DisplayName { get; }
    public string Builders { get; }
    public string Credits { get; }
    public IEnumerable<IRoom> Rooms => _rooms;
    public IEnumerable<IPlayer> Players => _rooms.SelectMany(x => x.People).OfType<IPlayableCharacter>().Select(x => x.ImpersonatedBy!);
    public IEnumerable<ICharacter> Characters => _rooms.SelectMany(x => x.People);
    public IEnumerable<IPlayableCharacter> PlayableCharacters => _rooms.SelectMany(x => x.People).OfType<IPlayableCharacter>();

    public bool AddRoom(IRoom room)
    {
        //if (room.Area != null)
        //{
        //    Logger.LogError($"Area.AddRoom: Room {room.DebugName}");
        //    return false;
        //}
        // TODO: some checks ?
        _rooms.Add(room);
        return true;
    }

    public bool RemoveRoom(IRoom room)
    {
        // TODO: some checks ?
        _rooms.Remove(room);
        return true;
    }

    #endregion
}
