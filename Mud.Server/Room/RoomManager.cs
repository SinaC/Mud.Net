using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Blueprints.Room;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Random;

namespace Mud.Server.Room;

[Export(typeof(IRoomManager)), Shared]
public class RoomManager : IRoomManager
{
    private ILogger<RoomManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IRandomManager RandomManager { get; }
    private IFlagsManager FlagsManager { get; }

    private int NullRoomId { get; }
    private int DefaultRecallRoomId { get; }
    private int DefaultDeathRoomId { get; }
    private int MudschoolRoomId { get; }

    // Null room is used to avoid setting char.room to null when deleting and is used as container when deleting item
    private IRoom? _nullRoom; // save a reference for further use

    private readonly Dictionary<int, RoomBlueprint> _roomBlueprints;
    private readonly List<IRoom> _rooms;

    public RoomManager(ILogger<RoomManager> logger, IServiceProvider serviceProvider, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IFlagsManager flagsManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        RandomManager = randomManager;

        NullRoomId = worldOptions.Value.BlueprintIds.NullRoom;
        DefaultRecallRoomId = worldOptions.Value.BlueprintIds.DefaultRecallRoom;
        DefaultDeathRoomId = worldOptions.Value.BlueprintIds.DefaultDeathRoom;
        MudschoolRoomId = worldOptions.Value.BlueprintIds.MudSchoolRoom;

        _roomBlueprints = [];
        _rooms = [];
        FlagsManager = flagsManager;
    }

    public IRoom NullRoom => _nullRoom = _nullRoom ?? Rooms.Single(x => x.Blueprint.Id == NullRoomId);

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
            Logger.LogError("Room blueprint duplicate {blueprintId}!!!", blueprint.Id);
        else
        {
            if (!FlagsManager.CheckFlags(blueprint.RoomFlags))
                Logger.LogError("Room blueprint {blueprintId} has invalid flags", blueprint.Id);
            _roomBlueprints.Add(blueprint.Id, blueprint);
        }
    }

    public IEnumerable<IRoom> Rooms => _rooms.Where(x => x.IsValid);

    public IRoom DefaultRecallRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == DefaultRecallRoomId)!;
    public IRoom DefaultDeathRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == DefaultDeathRoomId)!;
    public IRoom MudSchoolRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == MudschoolRoomId)!;

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
        var room = ServiceProvider.GetRequiredService<IRoom>();
        room.Initialize(Guid.NewGuid(), blueprint, area);
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
        //var auras = room.Auras.ToArray(); // clone
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
