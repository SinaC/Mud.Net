using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.Room;

public class RoomManager : IRoomManager
{
    private IServiceProvider ServiceProvider { get; }
    private ISettings Settings { get; }
    private IRandomManager RandomManager { get; }
    private IGameActionManager GameActionManager { get; }
    private IAbilityManager AbilityManager { get; }
    private ITimeManager TimeManager { get; }
    private IItemManager ItemManager { get; }
    private ICharacterManager CharacterManager { get; }

    // Null room is used to avoid setting char.room to null when deleting and is used as container when deleting item
    private IRoom? _nullRoom; // save a reference for further use

    private readonly Dictionary<int, RoomBlueprint> _roomBlueprints;
    private readonly List<IRoom> _rooms;

    public RoomManager(IServiceProvider serviceProvider, ISettings settings, IRandomManager randomManager, IGameActionManager gameActionManager, IAbilityManager abilityManager, ITimeManager timeManager, IItemManager itemManager, ICharacterManager characterManager)
    {
        ServiceProvider = serviceProvider;
        Settings = settings;
        RandomManager = randomManager;
        GameActionManager = gameActionManager;
        AbilityManager = abilityManager;
        TimeManager = timeManager;
        ItemManager = itemManager;
        CharacterManager = characterManager;

        _roomBlueprints = [];
        _rooms = [];
    }

    public IRoom NullRoom => _nullRoom = _nullRoom ?? Rooms.Single(x => x.Blueprint.Id == Settings.NullRoomId);

    public IReadOnlyCollection<RoomBlueprint> RoomBlueprints
        => _roomBlueprints.Values.ToList().AsReadOnly();

    public RoomBlueprint? GetRoomBlueprint(int id)
    {
        _roomBlueprints.TryGetValue(id, out var blueprint);
        return blueprint;
    }

    public void AddRoomBlueprint(RoomBlueprint blueprint)
    {
        if (_roomBlueprints.ContainsKey(blueprint.Id))
            Log.Default.WriteLine(LogLevels.Error, "Room blueprint duplicate {0}!!!", blueprint.Id);
        else
            _roomBlueprints.Add(blueprint.Id, blueprint);
    }

    public IEnumerable<IRoom> Rooms => _rooms.Where(x => x.IsValid);

    public IRoom DefaultRecallRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRecallRoomId)!;
    public IRoom DefaultDeathRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultDeathRoomId)!;
    public IRoom MudSchoolRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.MudSchoolRoomId)!;

    public IRoom GetRandomRoom(ICharacter character)
    {
        var nonPlayableCharacter = character as INonPlayableCharacter;
        return RandomManager.Random(Rooms.Where(x =>
            character.CanSee(x)
            && !x.IsPrivate
            && !x.RoomFlags.HasAny("Safe", "Private", "Solitary")
            && (nonPlayableCharacter == null || nonPlayableCharacter.ActFlags.IsSet("Aggressive") || !x.RoomFlags.IsSet("Law"))))!;
    }

    public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        var room = new Room(ServiceProvider, GameActionManager, AbilityManager, Settings, TimeManager, ItemManager, CharacterManager, Guid.NewGuid(), blueprint, area);
        room.Recompute();
        _rooms.Add(room);
        return room;
    }

    public IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction)
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        Exit from2To = new(blueprint, to);
        from.Exits[(int)direction] = from2To;
        return from2To;
    }

    public void RemoveRoom(IRoom room)
    {
        //// Remove auras
        //IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(room.Auras.ToList()); // clone
        //foreach (IAura aura in auras)
        //{
        //    aura.OnRemoved();
        //    room.RemoveAura(aura, false);
        //}
        //// no need to recompute
        ////
        //room.OnRemoved();
        throw new NotImplementedException();
    }

    public void Cleanup()
    {
        // NOP
    }
}
