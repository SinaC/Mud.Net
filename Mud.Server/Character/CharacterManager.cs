using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using System.Collections.ObjectModel;

namespace Mud.Server.Character;

public class CharacterManager : ICharacterManager
{
    private ILogger<CharacterManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IRoomManager RoomManager { get; }
    private IItemManager ItemManager { get; }

    private readonly Dictionary<int, CharacterBlueprintBase> _characterBlueprints;
    private readonly List<ICharacter> _characters;

    public CharacterManager(ILogger<CharacterManager> logger, IServiceProvider serviceProvider, IRoomManager roomManager, IItemManager itemManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        RoomManager = roomManager;
        ItemManager = itemManager;

        _characterBlueprints = [];
        _characters = [];
    }

    public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints
        => _characterBlueprints.Values.ToList().AsReadOnly();

    public CharacterBlueprintBase? GetCharacterBlueprint(int id)
    {
        {
            _characterBlueprints.TryGetValue(id, out var blueprint);
            return blueprint;
        }
    }

    public TBlueprint? GetCharacterBlueprint<TBlueprint>(int id)
        where TBlueprint : CharacterBlueprintBase
        => GetCharacterBlueprint(id) as TBlueprint;

    public void AddCharacterBlueprint(CharacterBlueprintBase blueprint)
    {
        if (_characterBlueprints.ContainsKey(blueprint.Id))
            Logger.LogError("Character blueprint duplicate {blueprintId}!!!", blueprint.Id);
        else
            _characterBlueprints.Add(blueprint.Id, blueprint);
    }

    public IEnumerable<ICharacter> Characters => _characters.Where(x => x.IsValid);

    public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();

    public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

    public IPlayableCharacter AddPlayableCharacter(Guid guid, PlayableCharacterData playableCharacterData, IPlayer player, IRoom room) // PC
    {
        var pc = ServiceProvider.GetRequiredService<IPlayableCharacter>();
        pc.Initialize(guid, playableCharacterData, player, room);
        _characters.Add(pc);
        pc.Recompute();
        room.Recompute();
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
        return npc;
    }

    public void RemoveCharacter(ICharacter character)
    {
        character.StopFighting(true);

        // Remove auras
        var auras = new ReadOnlyCollection<IAura>(character.Auras.ToList()); // clone
        foreach (var aura in auras)
        {
            aura.OnRemoved();
            character.RemoveAura(aura, false);
        }
        // no need to recompute

        // Remove content
        if (character.Inventory.Any())
        {
            var clonedInventory = new ReadOnlyCollection<IItem>(character.Inventory.ToList()); // clone because GetFromContainer change Content collection
            foreach (var item in clonedInventory)
                ItemManager.RemoveItem(item);
            // Remove equipments
            if (character.Equipments.Any(x => x.Item != null))
            {
                var equipment = new ReadOnlyCollection<IItem>(character.Equipments.Where(x => x.Item != null).Select(x => x.Item!).ToList()); // clone
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
