using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Blueprints.Character;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Character;

[Export(typeof(ICharacterManager)), Shared]
public class CharacterManager : ICharacterManager
{
    private ILogger<CharacterManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IRoomManager RoomManager { get; }
    private IItemManager ItemManager { get; }
    private IFlagsManager FlagsManager { get; }

    private readonly Dictionary<int, CharacterBlueprintBase> _characterBlueprints;
    private readonly List<ICharacter> _characters;

    public CharacterManager(ILogger<CharacterManager> logger, IServiceProvider serviceProvider, IRoomManager roomManager, IItemManager itemManager, IFlagsManager flagsManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        RoomManager = roomManager;
        ItemManager = itemManager;

        _characterBlueprints = [];
        _characters = [];
        FlagsManager = flagsManager;
    }

    public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints
        => _characterBlueprints.Values.ToList().AsReadOnly();

    public CharacterBlueprintBase? GetCharacterBlueprint(int id)
        => _characterBlueprints.GetValueOrDefault(id);

    public TBlueprint? GetCharacterBlueprint<TBlueprint>(int id)
        where TBlueprint : CharacterBlueprintBase
        => GetCharacterBlueprint(id) as TBlueprint;

    public void AddCharacterBlueprint(CharacterBlueprintBase blueprint)
    {
        if (!_characterBlueprints.TryAdd(blueprint.Id, blueprint))
            Logger.LogError("Character blueprint duplicate {blueprintId}!!!", blueprint.Id);
        else
        {
            var checkSuccess = true;
            checkSuccess &= FlagsManager.CheckFlags(blueprint.ActFlags);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.OffensiveFlags);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.AssistFlags);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.CharacterFlags);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.Immunities);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.Resistances);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.Vulnerabilities);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.ShieldFlags);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.BodyForms);
            checkSuccess &= FlagsManager.CheckFlags(blueprint.BodyParts);
            if (!checkSuccess)
                Logger.LogError("NPC blueprint {blueprintId} has invalid flags", blueprint.Id);
        }
    }

    public IEnumerable<ICharacter> Characters => _characters.Where(x => x.IsValid);

    public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();

    public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

    public IPlayableCharacter AddPlayableCharacter(Guid guid, AvatarData playableCharacterData, IPlayer player, IRoom room) // PC
    {
        var pc = ServiceProvider.GetRequiredService<IPlayableCharacter>();
        pc.Initialize(guid, playableCharacterData, player, room);
        _characters.Add(pc);
        pc.Recompute();
        room.Recompute();
        var checkSuccess = true;
        checkSuccess &= FlagsManager.CheckFlags(pc.CharacterFlags);
        checkSuccess &= FlagsManager.CheckFlags(pc.Immunities);
        checkSuccess &= FlagsManager.CheckFlags(pc.Resistances);
        checkSuccess &= FlagsManager.CheckFlags(pc.Vulnerabilities);
        checkSuccess &= FlagsManager.CheckFlags(pc.ShieldFlags);
        checkSuccess &= FlagsManager.CheckFlags(pc.BodyForms);
        checkSuccess &= FlagsManager.CheckFlags(pc.BodyParts);
        if (!checkSuccess)
            Logger.LogError("PC {name} has invalid flags", playableCharacterData.Name);
        return pc;
    }

    public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        var npc = ServiceProvider.GetRequiredService<INonPlayableCharacter>();
        npc.Initialize(guid, blueprint, room);
        _characters.Add(npc);
        npc.Recompute();
        room.Recompute();
        var checkSuccess = true;
        checkSuccess &= FlagsManager.CheckFlags(blueprint.ActFlags);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.OffensiveFlags);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.AssistFlags);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.CharacterFlags);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.Immunities);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.Resistances);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.Vulnerabilities);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.ShieldFlags);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.BodyForms);
        checkSuccess &= FlagsManager.CheckFlags(blueprint.BodyParts);
        if (!checkSuccess)
            Logger.LogError("NPC blueprint {blueprintId} has invalid flags", blueprint.Id);
        return npc;
    }

    public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room) // pet
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        var npc = ServiceProvider.GetRequiredService<INonPlayableCharacter>();
        npc.Initialize(guid, blueprint, petData, room);
        _characters.Add(npc);
        npc.Recompute();
        room.Recompute();
        var checkSuccess = true;
        checkSuccess &= FlagsManager.CheckFlags(npc.ActFlags);
        checkSuccess &= FlagsManager.CheckFlags(npc.OffensiveFlags);
        checkSuccess &= FlagsManager.CheckFlags(npc.AssistFlags);
        checkSuccess &= FlagsManager.CheckFlags(npc.CharacterFlags);
        checkSuccess &= FlagsManager.CheckFlags(npc.Immunities);
        checkSuccess &= FlagsManager.CheckFlags(npc.Resistances);
        checkSuccess &= FlagsManager.CheckFlags(npc.Vulnerabilities);
        checkSuccess &= FlagsManager.CheckFlags(npc.ShieldFlags);
        checkSuccess &= FlagsManager.CheckFlags(npc.BodyForms);
        checkSuccess &= FlagsManager.CheckFlags(npc.BodyParts);
        if (!checkSuccess)
            Logger.LogError("Pet blueprint {blueprintId} pet {name} has invalid flags", blueprint.Id, petData.Name);
        return npc;
    }

    public void RemoveCharacter(ICharacter character)
    {
        character.StopFighting(true);

        // Remove auras
        character.RemoveAuras(_ => true, false);

        // Remove content
        if (character.Inventory.Any())
        {
            var inventory = character.Inventory.ToArray(); // clone because GetFromContainer change Content collection
            foreach (var item in inventory)
                ItemManager.RemoveItem(item);
            // Remove equipments
            if (character.Equipments.Any(x => x.Item != null))
            {
                var equipment = character.Equipments.Where(x => x.Item != null).Select(x => x.Item!).ToArray(); // clone
                foreach (var item in equipment)
                    ItemManager.RemoveItem(item);
            }
        }
        // Move to NullRoom
        character.ChangeRoom(RoomManager.NullRoom, true);
        //
        character.OnRemoved();
        //_characters.Remove(character); will be removed in cleanup step
    }

    public void Cleanup()
    {
        if (_characters.Any(x => !x.IsValid))
            Logger.LogDebug("Cleaning {count} character(s)", _characters.Count(x => !x.IsValid));

        var charactersToRemove = _characters.Where(x => !x.IsValid).ToArray();
        foreach (var character in charactersToRemove)
            character.OnCleaned(); // definitive remove
        _characters.RemoveAll(x => !x.IsValid);
    }
}
