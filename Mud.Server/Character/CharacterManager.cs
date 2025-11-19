using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;
using Mud.Settings.Interfaces;
using System.Collections.ObjectModel;

namespace Mud.Server.Character;

public class CharacterManager : ICharacterManager
{
    private ILogger<CharacterManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IGameActionManager GameActionManager { get; }
    private ICommandParser CommandParser { get; }
    private IAbilityManager AbilityManager { get; }
    private ISettings Settings { get; }
    private IRandomManager RandomManager { get; }
    private ITableValues TableValues { get; }
    private IRoomManager RoomManager { get; }
    private IItemManager ItemManager { get; }
    private IAuraManager AuraManager { get; }
    private IWeaponEffectManager WeaponEffectManager { get; }
    private IWiznet Wiznet { get; }
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }
    private ITimeManager TimeManager { get; }
    private IQuestManager QuestManager { get; }

    private readonly Dictionary<int, CharacterBlueprintBase> _characterBlueprints;
    private readonly List<ICharacter> _characters;

    public CharacterManager(ILogger<CharacterManager> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRandomManager randomManager, ITableValues tableValues, IRoomManager roomManager, IItemManager itemManager, IAuraManager auraManager, IWeaponEffectManager weaponEffectManager, IWiznet wiznet, IRaceManager raceManager, IClassManager classManager, ITimeManager timeManager, IQuestManager questManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        GameActionManager = gameActionManager;
        CommandParser = commandParser;
        AbilityManager = abilityManager;
        Settings = settings;
        RandomManager = randomManager;
        TableValues = tableValues;
        RoomManager = roomManager;
        ItemManager = itemManager;
        AuraManager = auraManager;
        WeaponEffectManager = weaponEffectManager;
        Wiznet = wiznet;
        RaceManager = raceManager;
        ClassManager = classManager;
        TimeManager = timeManager;
        QuestManager = questManager;

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
        var character = new PlayableCharacter.PlayableCharacter(Logger, ServiceProvider, GameActionManager, CommandParser, AbilityManager, Settings, RandomManager, TableValues, RoomManager, ItemManager, this, AuraManager, WeaponEffectManager, Wiznet, RaceManager, ClassManager, TimeManager, QuestManager, guid, playableCharacterData, player, room);
        _characters.Add(character);
        character.Recompute();
        room.Recompute();
        return character;
    }

    public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        var character = new NonPlayableCharacter.NonPlayableCharacter(Logger, ServiceProvider, GameActionManager, CommandParser, AbilityManager, Settings, RandomManager, TableValues, RoomManager, ItemManager, this, AuraManager, WeaponEffectManager, Wiznet, RaceManager, ClassManager, guid, blueprint, room);
        _characters.Add(character);
        character.Recompute();
        room.Recompute();
        return character;
    }

    public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room) // pet
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        var character = new NonPlayableCharacter.NonPlayableCharacter(Logger, ServiceProvider, GameActionManager, CommandParser, AbilityManager, Settings, RandomManager, TableValues, RoomManager, ItemManager, this, AuraManager, WeaponEffectManager, Wiznet, RaceManager, ClassManager, guid, blueprint, petData, room);
        _characters.Add(character);
        character.Recompute();
        room.Recompute();
        return character;
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
