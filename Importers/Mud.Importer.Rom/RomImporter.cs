using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Blueprints.Area;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.Reset;
using Mud.Blueprints.Room;
using Mud.Flags;
using Mud.Flags.Interfaces;
using System.Diagnostics;
using Mud.Importer.Rom.Domain;
using Mud.Blueprints.Item.Affects;
using System.Reflection.Metadata.Ecma335;

namespace Mud.Importer.Rom;

[Export("RomImporter", typeof(IImporter)), Shared]
public class RomImporter : IImporter
{
    private ILogger<RomImporter> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private readonly List<AreaBlueprint> _areaBlueprints = [];
    private readonly List<RoomBlueprint> _roomBlueprints = [];
    private readonly List<ItemBlueprintBase> _itemBlueprints = [];
    private readonly List<CharacterBlueprintBase> _characterBlueprints = [];

    public IReadOnlyCollection<AreaBlueprint> Areas => _areaBlueprints.AsReadOnly();
    public IReadOnlyCollection<RoomBlueprint> Rooms => _roomBlueprints.AsReadOnly();
    public IReadOnlyCollection<ItemBlueprintBase> Items => _itemBlueprints.AsReadOnly();
    public IReadOnlyCollection<CharacterBlueprintBase> Characters => _characterBlueprints.AsReadOnly();

    public RomImporter(ILogger<RomImporter> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public void ImportByList(string path, string areaLst)
    {
        var loader = ServiceProvider.GetRequiredService<RomLoader>();
        var areaFilenames = File.ReadAllLines(Path.Combine(path, areaLst));
        foreach (var areaFilename in areaFilenames)
        {
            if (areaFilename.Contains("$"))
                break;
            if (areaFilename.StartsWith("-"))
                Logger.LogInformation("Skipping {area}", areaFilename);
            else
            {
                string areaFullName = Path.Combine(path, RemoveCommentIfAny(areaFilename));
                loader.Load(areaFullName);
                loader.Parse();
            }
        }

        Convert(loader);
    }

    public void Import(string path, params string[] filenames)
    {
        var loader = ServiceProvider.GetRequiredService<RomLoader>();
        foreach (var filename in filenames)
        {
            var fullName = Path.Combine(path, filename);
            loader.Load(fullName);
            loader.Parse();
        }

        Convert(loader);
    }

    public void Import(string path, IEnumerable<string> filenames)
    {
        var loader = ServiceProvider.GetRequiredService<RomLoader>();
        foreach (var filename in filenames)
        {
            var fullName = Path.Combine(path, filename);
            loader.Load(fullName);
            loader.Parse();
        }

        Convert(loader);
    }

    private void Convert(RomLoader loader)
    {
        foreach (var areaData in loader.Areas)
        {
            var areaBlueprint = ConvertArea(areaData);
            if (areaBlueprint != null)
                _areaBlueprints.Add(areaBlueprint);
        }

        foreach (var roomData in loader.Rooms)
        {
            var roomBlueprint = ConvertRoom(roomData);
            if (roomBlueprint != null)
                _roomBlueprints.Add(roomBlueprint);
        }

        foreach (var objectData in loader.Objects)
        {
            var itemBlueprint = ConvertObject(objectData);
            if (itemBlueprint != null)
                _itemBlueprints.Add(itemBlueprint);
        }

        foreach (var mobileData in loader.Mobiles)
        {
            var characterBlueprint = ConvertMobile(mobileData, _roomBlueprints);
            if (characterBlueprint != null)
                _characterBlueprints.Add(characterBlueprint);
        }
    }

    #region Area

    private AreaBlueprint ConvertArea(AreaData areaData)
    {
        if (_areaBlueprints.Any(x => x.Id == areaData.VNum))
            RaiseConvertException("Duplicate area Id {0}", areaData.VNum);
        return new AreaBlueprint
        {
            Id = areaData.VNum,
            Filename = areaData.FileName,
            Name = areaData.Name,
            Credits = areaData.Credits,
            MinId = areaData.MinVNum,
            MaxId = areaData.MaxVNum,
            Builders = areaData.Builders,
            Flags = ConvertAreaFlags(areaData.Flags),
            Security = areaData.Security
        };
    }

    private AreaFlags ConvertAreaFlags(long input)
    {
        var flags = new AreaFlags();
        if (IsSet(input, AREA_CHANGED)) flags.Set("Changed");
        if (IsSet(input, AREA_ADDED)) flags.Set("Added");
        if (IsSet(input, AREA_LOADING)) flags.Set("Loading");
        return flags;
    }

    // Area flags
    private const long AREA_NONE = 0;
    private const long AREA_CHANGED = 1;
    private const long AREA_ADDED = 2;
    private const long AREA_LOADING = 4;

    #endregion

    #region Room

    private RoomBlueprint ConvertRoom(RoomData roomData)
    {
        if (_roomBlueprints.Any(x => x.Id == roomData.VNum))
            RaiseConvertException("Duplicate room Id {0}", roomData.VNum);
        return new RoomBlueprint
        {
            Id = roomData.VNum,
            AreaId = roomData.AreaVnum,
            Name = roomData.Name,
            Description = roomData.Description,
            ExtraDescriptions = RoomBlueprint.BuildExtraDescriptions(roomData.ExtraDescr),
            RoomFlags = ConvertRoomFlags(roomData.Flags),
            SectorType = ConvertSectorTypes(roomData.Sector),
            HealRate = roomData.HealRate,
            ResourceRate = roomData.ManaRate,
            Exits = ConvertExits(roomData),
            Resets = ConvertResets(roomData).ToList(),
            MaxSize = null, // doesn't exist in Rom2.4b6
        };
    }

    private ExitBlueprint[] ConvertExits(RoomData roomData)
    {
        var blueprints = new ExitBlueprint[RoomData.MaxExits];
        for (var i = 0; i < RoomData.MaxExits; i++)
        {
            var exit = roomData.Exits[i];
            if (exit != null)
            {
                blueprints[i] = new ExitBlueprint
                {
                    Direction = (ExitDirections)i,
                    Destination = exit.DestinationVNum,
                    Description = exit.Description,
                    Key = exit.Key,
                    Keyword = exit.Keyword,
                    Flags = ConvertExitFlags(exit.ExitInfo)
                };
            }
            else
                blueprints[i] = default!;
        }

        return blueprints;
    }

    private SectorTypes ConvertSectorTypes(int sector)
    {
        switch (sector)
        {
            case SECT_INSIDE: return SectorTypes.Inside;
            case SECT_CITY: return SectorTypes.City;
            case SECT_FIELD: return SectorTypes.Field;
            case SECT_FOREST: return SectorTypes.Forest;
            case SECT_HILLS: return SectorTypes.Hills;
            case SECT_MOUNTAIN: return SectorTypes.Mountain;
            case SECT_WATER_SWIM: return SectorTypes.WaterSwim;
            case SECT_WATER_NOSWIM: return SectorTypes.WaterNoSwim;
            case SECT_AIR: return SectorTypes.Air;
            case SECT_DESERT: return SectorTypes.Desert;
            default: return SectorTypes.Inside;
        }
    }

    private IRoomFlags ConvertRoomFlags(long input)
    {
        var flags = new RoomFlags();
        if (IsSet(input, ROOM_DARK)) flags.Set("Dark");
        if (IsSet(input, ROOM_NO_MOB)) flags.Set("NoMob");
        if (IsSet(input, ROOM_INDOORS)) flags.Set("Indoors");
        if (IsSet(input, ROOM_PRIVATE)) flags.Set("Private");
        if (IsSet(input, ROOM_SAFE)) flags.Set("Safe");
        if (IsSet(input, ROOM_SOLITARY)) flags.Set("Solitary");
        if (IsSet(input, ROOM_PET_SHOP)) flags.Set("PetShop");
        if (IsSet(input, ROOM_NO_RECALL)) flags.Set("NoRecall");
        if (IsSet(input, ROOM_IMP_ONLY)) flags.Set("ImpOnly");
        if (IsSet(input, ROOM_GODS_ONLY)) flags.Set("GodsOnly");
        if (IsSet(input, ROOM_HEROES_ONLY)) flags.Set("HeroesOnly");
        if (IsSet(input, ROOM_NEWBIES_ONLY)) flags.Set("NewbiesOnly");
        if (IsSet(input, ROOM_LAW)) flags.Set("Law");
        if (IsSet(input, ROOM_NOWHERE)) flags.Set("NoWhere");

        return flags;
    }

    private ExitFlags ConvertExitFlags(long input)
    {
        var flags = new ExitFlags();
        if (IsSet(input, EX_ISDOOR)) flags.Set("Door");
        if (IsSet(input, EX_CLOSED)) flags.Set("Closed");
        if (IsSet(input, EX_LOCKED)) flags.Set("Locked");
        if (IsSet(input, EX_PICKPROOF)) flags.Set("PickProof");
        if (IsSet(input, EX_NOPASS)) flags.Set("NoPass");
        if (IsSet(input, EX_EASY)) flags.Set("Easy");
        if (IsSet(input, EX_HARD)) flags.Set("Hard");
        // EX_INFURIATING
        // EX_NOCLOSE
        // EX_NOLOCK
        if (flags.IsNone)
            flags.Set("Door"); // force door if another flag found
        return flags;
    }

    // Sector types
    private const int SECT_INSIDE = 0;
    private const int SECT_CITY = 1;
    private const int SECT_FIELD = 2;
    private const int SECT_FOREST = 3;
    private const int SECT_HILLS = 4;
    private const int SECT_MOUNTAIN = 5;
    private const int SECT_WATER_SWIM = 6;
    private const int SECT_WATER_NOSWIM = 7;
    private const int SECT_UNUSED = 8;
    private const int SECT_AIR = 9;
    private const int SECT_DESERT = 10;

    // Room flags
    private const long ROOM_DARK = RomLoader.A;
    private const long ROOM_NO_MOB = RomLoader.C;
    private const long ROOM_INDOORS = RomLoader.D;
    private const long ROOM_PRIVATE = RomLoader.J;
    private const long ROOM_SAFE = RomLoader.K;
    private const long ROOM_SOLITARY = RomLoader.L;
    private const long ROOM_PET_SHOP = RomLoader.M;
    private const long ROOM_NO_RECALL = RomLoader.N;
    private const long ROOM_IMP_ONLY = RomLoader.O;
    private const long ROOM_GODS_ONLY = RomLoader.P;
    private const long ROOM_HEROES_ONLY = RomLoader.Q;
    private const long ROOM_NEWBIES_ONLY = RomLoader.R;
    private const long ROOM_LAW = RomLoader.S;
    private const long ROOM_NOWHERE = RomLoader.T;

    // Exit flags
    private const long EX_ISDOOR = RomLoader.A;
    private const long EX_CLOSED = RomLoader.B;
    private const long EX_LOCKED = RomLoader.C;
    private const long EX_PICKPROOF = RomLoader.F;
    private const long EX_NOPASS = RomLoader.G;
    private const long EX_EASY = RomLoader.H;
    private const long EX_HARD = RomLoader.I;
    private const long EX_INFURIATING = RomLoader.J;
    private const long EX_NOCLOSE = RomLoader.K;
    private const long EX_NOLOCK = RomLoader.L;

    #endregion

    #region Reset

    private IEnumerable<ResetBase> ConvertResets(RoomData roomData)
    {
        foreach (ResetData reset in roomData.Resets)
        {
            switch (reset.Command)
            {
                case 'M':
                    Debug.Assert(reset.Arg3 == roomData.VNum, $"Reset M arg3 '{reset.Arg3}' should be equal to room id '{roomData.VNum}'.");
                    if (reset.Arg2 == 0)
                        Logger.LogWarning("Reset M arg2 (global limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    if (reset.Arg4 == 0)
                        Logger.LogWarning("Reset M arg4 (local limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    yield return new CharacterReset
                    {
                        RoomId = roomData.VNum,
                        CharacterId = reset.Arg1,
                        GlobalLimit = reset.Arg2,
                        LocalLimit = reset.Arg4
                    };
                    break;
                case 'O':
                    Debug.Assert(reset.Arg3 == roomData.VNum, $"Reset O arg3 '{reset.Arg3}' should be equal to room id '{roomData.VNum}'.");
                    if (reset.Arg2 == 0)
                        Logger.LogWarning("Reset O arg2 (global limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    yield return new ItemInRoomReset
                    {
                        RoomId = roomData.VNum,
                        ItemId = reset.Arg1,
                        GlobalLimit = reset.Arg2, // is Arg4 with OLC but arg4 is always set to 0 by load_resets
                        LocalLimit = 1,//reset.Arg4, in db.c/reset_area once we find the item we don't load another one
                    };
                    break;
                case 'P':
                    if (reset.Arg2 == 0)
                        Logger.LogWarning("Reset P arg2 (global limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    if (reset.Arg4 == 0)
                        Logger.LogWarning("Reset P arg4 (local limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    yield return new ItemInItemReset
                    {
                        RoomId = roomData.VNum,
                        ItemId = reset.Arg1,
                        ContainerId = reset.Arg3,
                        GlobalLimit = reset.Arg2,
                        LocalLimit = reset.Arg4,
                    };
                    break;
                case 'G':
                    if (reset.Arg2 == 0)
                        Logger.LogWarning("Reset G arg2 (global limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    yield return new ItemInCharacterReset
                    {
                        RoomId = roomData.VNum,
                        ItemId = reset.Arg1,
                        GlobalLimit = reset.Arg2,
                    };
                    break;
                case 'E':
                    if (reset.Arg2 == 0)
                        Logger.LogWarning("Reset E arg2 (global limit) is 0 for room id '{vnum}'.", roomData.VNum);
                    yield return new ItemInEquipmentReset
                    {
                        RoomId = roomData.VNum,
                        ItemId = reset.Arg1,
                        EquipmentSlot = ConvertResetDataWearLocation(reset.Arg3),
                        GlobalLimit = reset.Arg2,
                    };
                    break;
                case 'D':
                    yield return new DoorReset
                    {
                        RoomId = roomData.VNum,
                        ExitDirection = (ExitDirections)reset.Arg2,
                        Operation = (DoorOperations)reset.Arg3
                    };
                    break;
                case 'R':
                    yield return new RandomizeExitsReset
                    {
                        RoomId = roomData.VNum,
                        MaxDirections = reset.Arg2
                    };
                    break;
                default:
                    Logger.LogError("Unknown Reset {reset} for room {vnum}", reset.Command, roomData.VNum);
                    break;
            }
        }
    }

    private static EquipmentSlots ConvertResetDataWearLocation(int resetDataWearLocation)
    {
        switch (resetDataWearLocation)
        {
            case WEAR_NONE: return EquipmentSlots.None;
            case WEAR_LIGHT: return EquipmentSlots.Light;
            case WEAR_FINGER_L:
            case WEAR_FINGER_R: return EquipmentSlots.Ring;
            case WEAR_NECK_1:
            case WEAR_NECK_2: return EquipmentSlots.Amulet;
            case WEAR_BODY: return EquipmentSlots.Chest;
            case WEAR_HEAD: return EquipmentSlots.Head;
            case WEAR_LEGS: return EquipmentSlots.Legs;
            case WEAR_FEET: return EquipmentSlots.Feet;
            case WEAR_HANDS: return EquipmentSlots.Hands;
            case WEAR_ARMS: return EquipmentSlots.Arms;
            case WEAR_SHIELD: return EquipmentSlots.OffHand;
            case WEAR_ABOUT: return EquipmentSlots.Cloak;
            case WEAR_WAIST: return EquipmentSlots.Waist;
            case WEAR_WRIST_L:
            case WEAR_WRIST_R: return EquipmentSlots.Wrists;
            case WEAR_WIELD: return EquipmentSlots.MainHand;
            case WEAR_HOLD: return EquipmentSlots.OffHand;
            case WEAR_FLOAT: return EquipmentSlots.Float;
        }

        return EquipmentSlots.None;
    }

    // Reset wear location
    private const int WEAR_NONE = -1;
    private const int WEAR_LIGHT = 0;
    private const int WEAR_FINGER_L = 1;
    private const int WEAR_FINGER_R = 2;
    private const int WEAR_NECK_1 = 3;
    private const int WEAR_NECK_2 = 4;
    private const int WEAR_BODY = 5;
    private const int WEAR_HEAD = 6;
    private const int WEAR_LEGS = 7;
    private const int WEAR_FEET = 8;
    private const int WEAR_HANDS = 9;
    private const int WEAR_ARMS = 10;
    private const int WEAR_SHIELD = 11;
    private const int WEAR_ABOUT = 12;
    private const int WEAR_WAIST = 13;
    private const int WEAR_WRIST_L = 14;
    private const int WEAR_WRIST_R = 15;
    private const int WEAR_WIELD = 16;
    private const int WEAR_HOLD = 17;
    private const int WEAR_FLOAT = 18;
    private const int MAX_WEAR = 19;

    #endregion

    #region Object

    private ItemBlueprintBase ConvertObject(ObjectData objectData)
    {
        if (_itemBlueprints.Any(x => x.Id == objectData.VNum))
            RaiseConvertException("Duplicate object Id {0}", objectData.VNum);
        var itemAffects = ConvertItemAffects(objectData).ToArray();
        switch (objectData.ItemType)
        {
            case "light":
                return new ItemLightBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    DurationHours = System.Convert.ToInt32(objectData.Values[2]),
                };
            case "scroll":
                return new ItemScrollBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    SpellLevel = System.Convert.ToInt32(objectData.Values[0]),
                    Spell1 = System.Convert.ToString(objectData.Values[1]),
                    Spell2 = System.Convert.ToString(objectData.Values[2]),
                    Spell3 = System.Convert.ToString(objectData.Values[3]),
                    Spell4 = System.Convert.ToString(objectData.Values[4]),
                };
            case "wand":
                return new ItemWandBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    SpellLevel = System.Convert.ToInt32(objectData.Values[0]),
                    MaxChargeCount = System.Convert.ToInt32(objectData.Values[1]) == 0 ? System.Convert.ToInt32(objectData.Values[2]) : System.Convert.ToInt32(objectData.Values[1]),
                    CurrentChargeCount = System.Convert.ToInt32(objectData.Values[2]),
                    Spell = System.Convert.ToString(objectData.Values[3]),
                    AlreadyRecharged = System.Convert.ToInt32(objectData.Values[1]) == 0
                };
            case "staff":
                return new ItemStaffBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    SpellLevel = System.Convert.ToInt32(objectData.Values[0]),
                    MaxChargeCount = System.Convert.ToInt32(objectData.Values[1]) == 0 ? System.Convert.ToInt32(objectData.Values[2]) : System.Convert.ToInt32(objectData.Values[1]),
                    CurrentChargeCount = System.Convert.ToInt32(objectData.Values[2]),
                    Spell = System.Convert.ToString(objectData.Values[3]),
                    AlreadyRecharged = System.Convert.ToInt32(objectData.Values[1]) == 0
                };
            case "weapon":
                (SchoolTypes damageType, IWeaponFlags weaponFlags, string damageNoun) weaponInfo = ConvertWeaponDamageTypeFlagsAndNoun(objectData);
                return new ItemWeaponBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    Type = ConvertWeaponType(objectData),
                    DiceCount = System.Convert.ToInt32(objectData.Values[1]),
                    DiceValue = System.Convert.ToInt32(objectData.Values[2]),
                    DamageType = weaponInfo.damageType,
                    Flags = weaponInfo.weaponFlags,
                    DamageNoun = weaponInfo.damageNoun,
                };
            case "treasure":
                return new ItemTreasureBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "armor":
                WearLocations wearLocations = ConvertWearLocation(objectData);
                if (wearLocations == WearLocations.Shield)
                    return new ItemShieldBlueprint
                    {
                        Id = objectData.VNum,
                        Name = objectData.Name,
                        ShortDescription = objectData.ShortDescr,
                        Description = objectData.Description,
                        ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                        ItemAffects = itemAffects,
                        Cost = System.Convert.ToInt32(objectData.Cost),
                        Level = objectData.Level,
                        Weight = objectData.Weight,
                        WearLocation = ConvertWearLocation(objectData),
                        ItemFlags = ConvertExtraFlags(objectData),
                        NoTake = IsNoTake(objectData),
                        Armor = objectData.Values.Take(4).Sum(System.Convert.ToInt32), // TODO
                    };
                else
                    return new ItemArmorBlueprint
                    {
                        Id = objectData.VNum,
                        Name = objectData.Name,
                        ShortDescription = objectData.ShortDescr,
                        Description = objectData.Description,
                        ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                        ItemAffects = itemAffects,
                        Cost = System.Convert.ToInt32(objectData.Cost),
                        Level = objectData.Level,
                        Weight = objectData.Weight,
                        WearLocation = ConvertWearLocation(objectData),
                        ItemFlags = ConvertExtraFlags(objectData),
                        NoTake = IsNoTake(objectData),
                        Pierce = System.Convert.ToInt32(objectData.Values[0]),
                        Bash = System.Convert.ToInt32(objectData.Values[1]),
                        Slash = System.Convert.ToInt32(objectData.Values[2]),
                        Exotic = System.Convert.ToInt32(objectData.Values[3]),
                    };
            case "potion":
                return new ItemPotionBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    SpellLevel = System.Convert.ToInt32(objectData.Values[0]),
                    Spell1 = System.Convert.ToString(objectData.Values[1]),
                    Spell2 = System.Convert.ToString(objectData.Values[2]),
                    Spell3 = System.Convert.ToString(objectData.Values[3]),
                    Spell4 = System.Convert.ToString(objectData.Values[4]),
                };
            case "clothing":
                return new ItemClothingBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "furniture":
                return new ItemFurnitureBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    MaxPeople = System.Convert.ToInt32(objectData.Values[0]),
                    MaxWeight = System.Convert.ToInt32(objectData.Values[1]),
                    FurnitureActions = ConvertFurnitureActions(objectData),
                    FurniturePlacePreposition = ConvertFurniturePreposition(objectData),
                    HealBonus = System.Convert.ToInt32(objectData.Values[3]),
                    ResourceBonus = System.Convert.ToInt32(objectData.Values[4]),
                };
            case "trash":
                return new ItemTrashBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "container":
                return new ItemContainerBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    MaxItems = 99,
                    MaxWeight = System.Convert.ToInt32(objectData.Values[0]),
                    ContainerFlags = ConvertContainerFlags(objectData),
                    Key = System.Convert.ToInt32(objectData.Values[2]),
                    MaxWeightPerItem = System.Convert.ToInt32(objectData.Values[3]),
                    WeightMultiplier = System.Convert.ToInt32(objectData.Values[4]),
                };
            case "drink":
                return new ItemDrinkContainerBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    MaxLiquidAmount = System.Convert.ToInt32(objectData.Values[0]),
                    CurrentLiquidAmount = System.Convert.ToInt32(objectData.Values[1]),
                    LiquidType = objectData.Values[2].ToString(),
                    IsPoisoned = System.Convert.ToInt32(objectData.Values[3]) != 0,
                };
            case "key":
                return new ItemKeyBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "food":
                return new ItemFoodBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    FullHours = System.Convert.ToInt32(objectData.Values[0]),
                    HungerHours = System.Convert.ToInt32(objectData.Values[1]),
                    IsPoisoned = System.Convert.ToInt32(objectData.Values[3]) != 0,
                };
            case "money":
                return new ItemMoneyBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    SilverCoins = System.Convert.ToInt64(objectData.Values[0]),
                    GoldCoins = System.Convert.ToInt64(objectData.Values[1]),
                };
            case "boat":
                return new ItemBoatBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "npc_corpse":
            case "pc_corpse":
                return new ItemCorpseBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "fountain":
                return new ItemFountainBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    LiquidType = objectData.Values[2].ToString(),
                };
            case "pill":
                return new ItemPillBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    SpellLevel = System.Convert.ToInt32(objectData.Values[0]),
                    Spell1 = System.Convert.ToString(objectData.Values[1]),
                    Spell2 = System.Convert.ToString(objectData.Values[2]),
                    Spell3 = System.Convert.ToString(objectData.Values[3]),
                    Spell4 = System.Convert.ToString(objectData.Values[4]),
                };
            //TODO: case "protect":
            case "map":
                return new ItemMapBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "portal":
                return new ItemPortalBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    Destination = System.Convert.ToInt32(objectData.Values[3]) <= 0 ? -1 : System.Convert.ToInt32(objectData.Values[3]),
                    PortalFlags = ConvertPortalFlags(objectData),
                };
            case "warp_stone":
                return new ItemWarpStoneBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            //TODO: case "room_key":
            case "gem":
                return new ItemGemBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "jewelry":
                return new ItemJewelryBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            case "jukebox":
                return new ItemJukeboxBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                    ItemAffects = itemAffects,
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            default:
                Logger.LogWarning("ItemBlueprint cannot be created: [{vnum}] [{type}] [{wearFlags}] : {name}", objectData.VNum, objectData.ItemType, objectData.WearFlags, objectData.Name);
                break;
        }

        return null!;
    }

    private IEnumerable<ItemAffectBase> ConvertItemAffects(ObjectData objectData)
    {
        foreach (var objectAffect in objectData.Affects)
        {
            switch (objectAffect.Where)
            {
                case ObjectAffect.WhereToAttributeOrResource:
                    if (objectAffect.Location == 6)
                        yield return new ItemAffectSex
                        {
                            Level = objectAffect.Level,
                            Sex = (Sex)objectAffect.Modifier
                        };
                    else if (objectAffect.Location >= 12 && objectAffect.Location <= 14)
                    {
                        var resourceKind = ConvertResourceKind(objectAffect.Location);
                        if (resourceKind is not null)
                        {
                            yield return new ItemAffectResource
                            {
                                Level = objectAffect.Level,
                                Location = resourceKind.Value,
                                Modifier = objectAffect.Modifier,
                            };
                        }
                        else
                            Logger.LogError("Item [{vnum}]: invalid ResourceKind affect {location}", objectData.VNum, objectAffect.Location);
                    }
                    else
                    {
                        var attribute = ConvertAffectCharacterAttribute(objectAffect.Location);
                        if (attribute != CharacterAttributeAffectLocations.None)
                        {
                            yield return new ItemAffectCharacterAttribute
                            {
                                Level = objectAffect.Level,
                                Attribute = attribute,
                                Modifier = objectAffect.Modifier,
                            };
                        }
                        else
                            Logger.LogError("Item [{vnum}]: invalid attribute affect {location}", objectData.VNum, objectAffect.Location);
                    }
                    break;
                case ObjectAffect.WhereToAffects:
                    var (characterFlags, shieldFlags) = ConvertCharacterFlags(objectAffect.BitVector);
                    if (!characterFlags.IsNone)
                        yield return new ItemAffectCharacterFlags
                        {
                            Level = objectAffect.Level,
                            CharacterFlags = characterFlags,
                        };
                    if (!shieldFlags.IsNone)
                        yield return new ItemAffectShieldFlags
                        {
                            Level = objectAffect.Level,
                            ShieldFlags = shieldFlags,
                        };
                    if (characterFlags.IsNone && shieldFlags.IsNone)
                        Logger.LogError("Item [{vnum}]: invalid affect/shield flags {flags}", objectData.VNum, objectAffect.BitVector);
                    break;
                case ObjectAffect.WhereToImmune:
                    yield return new ItemAffectImmFlags
                    {
                        Level = objectAffect.Level,
                        IRVFlags = ConvertIRV(objectAffect.BitVector)
                    };
                    break;
                case ObjectAffect.WhereToResist:
                    yield return new ItemAffectResFlags
                    {
                        Level = objectAffect.Level,
                        IRVFlags = ConvertIRV(objectAffect.BitVector)
                    };
                    break;
                case ObjectAffect.WhereToVuln:
                    yield return new ItemAffectVulnFlags
                    {
                        Level = objectAffect.Level,
                        IRVFlags = ConvertIRV(objectAffect.BitVector)
                    };
                    break;
            }
        }
    }

    private ResourceKinds? ConvertResourceKind(int location)
    {
        ResourceKinds? resourceKind = location switch
        {
            12 => ResourceKinds.Mana,
            13 => ResourceKinds.HitPoints,
            14 => ResourceKinds.MovePoints,
            _ => null!
        };
        return resourceKind;
    }

    private CharacterAttributeAffectLocations ConvertAffectCharacterAttribute(int location)
    {
        var attribute = location switch
        {
            1 => CharacterAttributeAffectLocations.Strength,
            2 => CharacterAttributeAffectLocations.Dexterity,
            3 => CharacterAttributeAffectLocations.Intelligence,
            4 => CharacterAttributeAffectLocations.Wisdom,
            5 => CharacterAttributeAffectLocations.Constitution,
            17 => CharacterAttributeAffectLocations.AllArmor,
            18 => CharacterAttributeAffectLocations.HitRoll,
            19 => CharacterAttributeAffectLocations.DamRoll,
            20 => CharacterAttributeAffectLocations.SavingThrow, // all saves
            21 => CharacterAttributeAffectLocations.SavingThrow, // save rod
            22 => CharacterAttributeAffectLocations.SavingThrow, // save petrification
            23 => CharacterAttributeAffectLocations.SavingThrow, // save breath
            24 => CharacterAttributeAffectLocations.SavingThrow, // save spell
            25 => CharacterAttributeAffectLocations.SavingThrow, // save spell effect
            _ => CharacterAttributeAffectLocations.None
        };
        return attribute;
    }

    private bool IsNoTake(ObjectData objectData) => (objectData.WearFlags & ITEM_TAKE) != ITEM_TAKE;

    private WearLocations ConvertWearLocation(ObjectData objectData) // in Rom2.4 it's a flag but in our code it a single value
    {
        if (objectData.ItemType == "light")
            return WearLocations.Light;
        switch (objectData.WearFlags & ~1 /*remove TAKE*/)
        {
            case 0: return WearLocations.None;
            case ITEM_WEAR_FINGER: return WearLocations.Ring;
            case ITEM_WEAR_NECK: return WearLocations.Amulet;
            case ITEM_WEAR_BODY: return WearLocations.Chest;
            case ITEM_WEAR_HEAD: return WearLocations.Head;
            case ITEM_WEAR_LEGS: return WearLocations.Legs;
            case ITEM_WEAR_FEET: return WearLocations.Feet;
            case ITEM_WEAR_HANDS: return WearLocations.Hands;
            case ITEM_WEAR_ARMS: return WearLocations.Arms;
            case ITEM_WEAR_SHIELD: return WearLocations.Shield;
            case ITEM_WEAR_ABOUT: return WearLocations.Cloak;
            case ITEM_WEAR_WAIST: return WearLocations.Waist;
            case ITEM_WEAR_WRIST: return WearLocations.Wrists;
            case ITEM_WIELD:
                if (objectData.ItemType == "weapon" && IsSet(objectData.Values[4] == null ? 0L : System.Convert.ToInt64(objectData.Values[4]), WEAPON_TWO_HANDS)) // Two-hands
                    return WearLocations.Wield2H;
                return WearLocations.Wield;
            case ITEM_HOLD: return WearLocations.Hold;
            //TODO: ITEM_NO_SAC
            case ITEM_WEAR_FLOAT: return WearLocations.Float;
        }

        Logger.LogWarning("Unknown wear location: {wearFlags} for item {vnum}", objectData.WearFlags, objectData.VNum);
        return WearLocations.None;
    }

    private IItemFlags ConvertExtraFlags(ObjectData objectData)
    {
        var itemFlags = ConvertExtraFlags(objectData.ExtraFlags);

        if (IsSet(objectData.WearFlags, ITEM_NO_SAC)) itemFlags.Set("NoSacrifice");

        return itemFlags;
    }

    private IItemFlags ConvertExtraFlags(long extraFlags)
    {
        var itemFlags = new ItemFlags();
        if (IsSet(extraFlags, ITEM_GLOW)) itemFlags.Set("Glowing");
        if (IsSet(extraFlags, ITEM_HUM)) itemFlags.Set("Humming");
        if (IsSet(extraFlags, ITEM_DARK)) itemFlags.Set("Dark");
        if (IsSet(extraFlags, ITEM_LOCK)) itemFlags.Set("Lock");
        if (IsSet(extraFlags, ITEM_EVIL)) itemFlags.Set("Evil");
        if (IsSet(extraFlags, ITEM_INVIS)) itemFlags.Set("Invis");
        if (IsSet(extraFlags, ITEM_MAGIC)) itemFlags.Set("Magic");
        if (IsSet(extraFlags, ITEM_NODROP)) itemFlags.Set("NoDrop");
        if (IsSet(extraFlags, ITEM_BLESS)) itemFlags.Set("Bless");
        if (IsSet(extraFlags, ITEM_ANTI_GOOD)) itemFlags.Set("AntiGood");
        if (IsSet(extraFlags, ITEM_ANTI_EVIL)) itemFlags.Set("AntiEvil");
        if (IsSet(extraFlags, ITEM_ANTI_NEUTRAL)) itemFlags.Set("AntiNeutral");
        if (IsSet(extraFlags, ITEM_NOREMOVE)) itemFlags.Set("NoRemove");
        if (IsSet(extraFlags, ITEM_INVENTORY)) itemFlags.Set("Inventory");
        if (IsSet(extraFlags, ITEM_NOPURGE)) itemFlags.Set("NoPurge");
        if (IsSet(extraFlags, ITEM_ROT_DEATH)) itemFlags.Set("RotDeath");
        if (IsSet(extraFlags, ITEM_VIS_DEATH)) itemFlags.Set("VisibleDeath");
        if (IsSet(extraFlags, ITEM_NONMETAL)) itemFlags.Set("NonMetal");
        if (IsSet(extraFlags, ITEM_NOLOCATE)) itemFlags.Set("NoLocate");
        if (IsSet(extraFlags, ITEM_MELT_DROP)) itemFlags.Set("MeltOnDrop");
        if (IsSet(extraFlags, ITEM_HAD_TIMER)) itemFlags.Set("HadTimer");
        if (IsSet(extraFlags, ITEM_SELL_EXTRACT)) itemFlags.Set("SellExtract");
        if (IsSet(extraFlags, ITEM_BURN_PROOF)) itemFlags.Set("BurnProof");
        if (IsSet(extraFlags, ITEM_NOUNCURSE)) itemFlags.Set("NoUncurse");
        return itemFlags;
    }

    private WeaponTypes ConvertWeaponType(ObjectData objectData)
    {
        var weaponType = (string)objectData.Values[0];
        switch (weaponType)
        {
            case "exotic": return WeaponTypes.Exotic;
            case "sword": return WeaponTypes.Sword;
            case "dagger": return WeaponTypes.Dagger;
            case "spear": return WeaponTypes.Spear;
            case "staff": return WeaponTypes.Staff; // spear in rom2.4
            case "mace": return WeaponTypes.Mace;
            case "axe": return WeaponTypes.Axe;
            case "flail": return WeaponTypes.Flail;
            case "whip": return WeaponTypes.Whip;
            case "polearm": return WeaponTypes.Polearm;
        }

        Logger.LogWarning("Unknown weapon type: {weaponType} for item {vnum}", weaponType, objectData.VNum);
        return WeaponTypes.Exotic;
    }

    private (SchoolTypes schoolType, IWeaponFlags weaponFlags, string damageNoun) ConvertWeaponDamageTypeFlagsAndNoun(ObjectData objectData)
    {
        var attackTable = (string)objectData.Values[3];
        var schoolType = SchoolTypes.None;
        var damageNoun = attackTable;
        (string name, string noun, int damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == attackTable);
        if (!attackTableEntry.Equals(default))
        {
            schoolType = ConvertDamageType(attackTableEntry.damType, $"item {objectData.VNum}");
            damageNoun = attackTableEntry.noun;
        }

        var weaponType2 = objectData.Values[4] == null ? 0L : System.Convert.ToInt64(objectData.Values[4]);
        var weaponFlags = new WeaponFlags();
        if (IsSet(weaponType2, WEAPON_FLAMING)) weaponFlags.Set("Flaming");
        if (IsSet(weaponType2, WEAPON_FROST)) weaponFlags.Set("Frost");
        if (IsSet(weaponType2, WEAPON_VAMPIRIC)) weaponFlags.Set("Vampiric");
        if (IsSet(weaponType2, WEAPON_SHARP)) weaponFlags.Set("Sharp");
        if (IsSet(weaponType2, WEAPON_VORPAL)) weaponFlags.Set("Vorpal");
        if (IsSet(weaponType2, WEAPON_TWO_HANDS)) weaponFlags.Set("TwoHands");
        if (IsSet(weaponType2, WEAPON_SHOCKING)) weaponFlags.Set("Shocking");
        if (IsSet(weaponType2, WEAPON_POISON)) weaponFlags.Set("Poison");

        //
        return (schoolType, weaponFlags, damageNoun);
    }

    private PortalFlags ConvertPortalFlags(ObjectData objectData)
    {
        // v1: exit flags
        // v2: gate flags
        var flags = new PortalFlags();
        long v1 = System.Convert.ToInt64(objectData.Values[1]);
        if (IsSet(v1, EX_CLOSED)) flags.Set("Closed");
        if (IsSet(v1, EX_LOCKED)) flags.Set("Locked");
        if (IsSet(v1, EX_PICKPROOF)) flags.Set("PickProof");
        if (IsSet(v1, EX_EASY)) flags.Set("Easy");
        if (IsSet(v1, EX_HARD)) flags.Set("Hard");
        if (IsSet(v1, EX_NOCLOSE)) flags.Set("NoClose");
        if (IsSet(v1, EX_NOLOCK)) flags.Set("NoLock");
        long v2 = System.Convert.ToInt32(objectData.Values[2]);
        if (IsSet(v2, GATE_NOCURSE)) flags.Set("NoCurse");
        if (IsSet(v2, GATE_GOWITH)) flags.Set("GoWith");
        if (IsSet(v2, GATE_BUGGY)) flags.Set("Buggy");
        if (IsSet(v2, GATE_RANDOM)) flags.Set("Random");
        return flags;
    }

    private ContainerFlags ConvertContainerFlags(ObjectData objectData)
    {
        var flags = new ContainerFlags();
        long v1 = System.Convert.ToInt64(objectData.Values[1]);
        if (!IsSet(v1, CONT_CLOSEABLE)) flags.Set("NoClose");
        if (IsSet(v1, CONT_PICKPROOF)) flags.Set("PickProof");
        if (IsSet(v1, CONT_CLOSED)) flags.Set("Closed");
        if (IsSet(v1, CONT_LOCKED)) flags.Set("Locked");
        long v2 = System.Convert.ToInt64(objectData.Values[2]);
        if (v2 <= 0) flags.Set("NoLock");
        return flags;
    }

    private FurnitureActions ConvertFurnitureActions(ObjectData objectData)
    {
        var actions = new FurnitureActions();
        int flag = objectData.Values[2] == null ? 0 : System.Convert.ToInt32(objectData.Values[2]);
        if (IsSet(flag, STAND_AT) || IsSet(flag, STAND_ON) || IsSet(flag, STAND_IN)) actions.Set("Stand");
        if (IsSet(flag, SIT_AT) || IsSet(flag, SIT_ON) || IsSet(flag, SIT_IN)) actions.Set("Sit");
        if (IsSet(flag, REST_AT) || IsSet(flag, REST_ON) || IsSet(flag, REST_IN) || IsSet(flag, SLEEP_ON)) actions.Set("Rest");
        if (IsSet(flag, SLEEP_AT) || IsSet(flag, SLEEP_ON) || IsSet(flag, SLEEP_IN) || IsSet(flag, SLEEP_ON)) actions.Set("Sleep");
        return actions;
    }

    private FurniturePlacePrepositions ConvertFurniturePreposition(ObjectData objectData)
    {
        var flag = objectData.Values[2] == null ? 0 : System.Convert.ToInt32(objectData.Values[2]);
        if (flag == 0)
            return FurniturePlacePrepositions.None;
        if (IsSet(flag, STAND_AT) || IsSet(flag, SIT_AT) || IsSet(flag, REST_AT) || IsSet(flag, SLEEP_AT)) return FurniturePlacePrepositions.At;
        if (IsSet(flag, STAND_ON) || IsSet(flag, SIT_ON) || IsSet(flag, REST_ON) || IsSet(flag, SLEEP_ON)) return FurniturePlacePrepositions.On;
        if (IsSet(flag, STAND_IN) || IsSet(flag, SIT_IN) || IsSet(flag, REST_IN) || IsSet(flag, SLEEP_IN)) return FurniturePlacePrepositions.In;
        Logger.LogWarning("Unknown Furniture preposition {flag} for item {vnum}", flag, objectData.VNum);
        return FurniturePlacePrepositions.None;
    }

    // Wear flags
    private const long ITEM_TAKE = RomLoader.A;
    private const long ITEM_WEAR_FINGER = RomLoader.B;
    private const long ITEM_WEAR_NECK = RomLoader.C;
    private const long ITEM_WEAR_BODY = RomLoader.D;
    private const long ITEM_WEAR_HEAD = RomLoader.E;
    private const long ITEM_WEAR_LEGS = RomLoader.F;
    private const long ITEM_WEAR_FEET = RomLoader.G;
    private const long ITEM_WEAR_HANDS = RomLoader.H;
    private const long ITEM_WEAR_ARMS = RomLoader.I;
    private const long ITEM_WEAR_SHIELD = RomLoader.J;
    private const long ITEM_WEAR_ABOUT = RomLoader.K;
    private const long ITEM_WEAR_WAIST = RomLoader.L;
    private const long ITEM_WEAR_WRIST = RomLoader.M;
    private const long ITEM_WIELD = RomLoader.N;
    private const long ITEM_HOLD = RomLoader.O;
    private const long ITEM_NO_SAC = RomLoader.P;
    private const long ITEM_WEAR_FLOAT = RomLoader.Q;

    // weapon class
    //#define WEAPON_EXOTIC		0
    //#define WEAPON_SWORD		1
    //#define WEAPON_DAGGER		2
    //#define WEAPON_SPEAR		3
    //#define WEAPON_MACE		4
    //#define WEAPON_AXE		5
    //#define WEAPON_FLAIL		6
    //#define WEAPON_WHIP		7	
    //#define WEAPON_POLEARM		8

    // weapon types
    private const long WEAPON_FLAMING = RomLoader.A;
    private const long WEAPON_FROST = RomLoader.B;
    private const long WEAPON_VAMPIRIC = RomLoader.C;
    private const long WEAPON_SHARP = RomLoader.D;
    private const long WEAPON_VORPAL = RomLoader.E;
    private const long WEAPON_TWO_HANDS = RomLoader.F;
    private const long WEAPON_SHOCKING = RomLoader.G;
    private const long WEAPON_POISON = RomLoader.H;

    // extra flags
    private const long ITEM_GLOW = RomLoader.A;
    private const long ITEM_HUM = RomLoader.B;
    private const long ITEM_DARK = RomLoader.C;
    private const long ITEM_LOCK = RomLoader.D;
    private const long ITEM_EVIL = RomLoader.E;
    private const long ITEM_INVIS = RomLoader.F;
    private const long ITEM_MAGIC = RomLoader.G;
    private const long ITEM_NODROP = RomLoader.H;
    private const long ITEM_BLESS = RomLoader.I;
    private const long ITEM_ANTI_GOOD = RomLoader.J;
    private const long ITEM_ANTI_EVIL = RomLoader.K;
    private const long ITEM_ANTI_NEUTRAL = RomLoader.L;
    private const long ITEM_NOREMOVE = RomLoader.M;
    private const long ITEM_INVENTORY = RomLoader.N;
    private const long ITEM_NOPURGE = RomLoader.O;
    private const long ITEM_ROT_DEATH = RomLoader.P;
    private const long ITEM_VIS_DEATH = RomLoader.Q;
    private const long ITEM_NONMETAL = RomLoader.S;
    private const long ITEM_NOLOCATE = RomLoader.T;
    private const long ITEM_MELT_DROP = RomLoader.U;
    private const long ITEM_HAD_TIMER = RomLoader.V;
    private const long ITEM_SELL_EXTRACT = RomLoader.W;
    private const long ITEM_BURN_PROOF = RomLoader.Y;
    private const long ITEM_NOUNCURSE = RomLoader.Z;

    // portal flags
    private const long GATE_NORMAL_EXIT = RomLoader.A;
    private const long GATE_NOCURSE = RomLoader.B;
    private const long GATE_GOWITH = RomLoader.C;
    private const long GATE_BUGGY = RomLoader.D;
    private const long GATE_RANDOM = RomLoader.E;

    // container flags
    private const long CONT_CLOSEABLE = 1;
    private const long CONT_PICKPROOF = 2;
    private const long CONT_CLOSED = 4;
    private const long CONT_LOCKED = 8;
    private const long CONT_PUT_ON = 16;

    // furniture flags
    private const long STAND_AT = RomLoader.A;
    private const long STAND_ON = RomLoader.B;
    private const long STAND_IN = RomLoader.C;
    private const long SIT_AT = RomLoader.D;
    private const long SIT_ON = RomLoader.E;
    private const long SIT_IN = RomLoader.F;
    private const long REST_AT = RomLoader.G;
    private const long REST_ON = RomLoader.H;
    private const long REST_IN = RomLoader.I;
    private const long SLEEP_AT = RomLoader.J;
    private const long SLEEP_ON = RomLoader.K;
    private const long SLEEP_IN = RomLoader.L;
    private const long PUT_AT = RomLoader.M;
    private const long PUT_ON = RomLoader.N;
    private const long PUT_IN = RomLoader.O;
    private const long PUT_INSIDE = RomLoader.P;

    #endregion

    #region Mobile

    private CharacterBlueprintBase ConvertMobile(MobileData mobileData, IEnumerable<RoomBlueprint> roomBlueprints)
    {
        if (_characterBlueprints.Any(x => x.Id == mobileData.VNum))
            RaiseConvertException("Duplicate mobile Id {0}", mobileData.VNum);

        var schoolType = SchoolTypes.None;
        var damageNoun = mobileData.DamageType;
        (string name, string noun, int damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == mobileData.DamageType);
        if (!attackTableEntry.Equals(default))
        {
            schoolType = ConvertDamageType(attackTableEntry.damType, $"mob {mobileData.VNum}");
            damageNoun = attackTableEntry.noun;
        }

        var (offensiveFlags, assistFlags) = ConvertOffensiveFlags(mobileData.OffFlags);
        var (characterFlags, shieldFlags) = ConvertCharacterFlags(mobileData.AffectedBy);

        // search a room flagged as pet_shop with mobile vnum in resets
        // sold pets are found in room vnum+1 (except for room 9621 which is linked to 9706!!)
        var petShopRoom = roomBlueprints.FirstOrDefault(x => x.RoomFlags.IsSet("petshop") && x.Resets.Count > 0 && x.Resets.OfType<CharacterReset>().Any(cr => cr.CharacterId == mobileData.VNum));
        if (petShopRoom != null)
        {
            var linkedPetShopRoomId = petShopRoom.Id == 9621 // exception for new thalos
                ? 9706
                : petShopRoom.Id + 1;
            var linkedPetShopRoom = roomBlueprints.SingleOrDefault(x => x.Id == linkedPetShopRoomId);
            if (linkedPetShopRoom != null)
            {
                var petBlueprintIds = linkedPetShopRoom.Resets.OfType<CharacterReset>().Select(x => x.CharacterId).ToList();

                return new CharacterPetShopBlueprint
                {
                    Id = mobileData.VNum,
                    Name = mobileData.Name,
                    Description = mobileData.Description,
                    Level = mobileData.Level,
                    LongDescription = mobileData.LongDescr,
                    ShortDescription = mobileData.ShortDescr,
                    Sex = ConvertSex(mobileData),
                    Size = ConvertSize(mobileData),
                    Wealth = mobileData.Wealth,
                    Alignment = mobileData.Alignment,
                    DamageNoun = damageNoun,
                    DamageType = schoolType,
                    DamageDiceCount = mobileData.Damage[0],
                    DamageDiceValue = mobileData.Damage[1],
                    DamageDiceBonus = mobileData.Damage[2],
                    HitPointDiceCount = mobileData.Hit[0],
                    HitPointDiceValue = mobileData.Hit[1],
                    HitPointDiceBonus = mobileData.Hit[2],
                    ManaDiceCount = mobileData.Mana[0],
                    ManaDiceValue = mobileData.Mana[1],
                    ManaDiceBonus = mobileData.Mana[2],
                    HitRollBonus = mobileData.HitRoll,
                    ArmorPierce = mobileData.Armor[0],
                    ArmorBash = mobileData.Armor[1],
                    ArmorSlash = mobileData.Armor[2],
                    ArmorExotic = mobileData.Armor[3],
                    CharacterFlags = characterFlags,
                    ActFlags = ConvertActFlags(mobileData.Act),
                    OffensiveFlags = offensiveFlags,
                    AssistFlags = assistFlags,
                    Immunities = ConvertIRV(mobileData.ImmFlags),
                    Resistances = ConvertIRV(mobileData.ResFlags),
                    Vulnerabilities = ConvertIRV(mobileData.VulnFlags),
                    ShieldFlags = shieldFlags,
                    StartPosition = ConvertPosition(mobileData, mobileData.StartPos),
                    DefaultPosition = ConvertPosition(mobileData, mobileData.DefaultPos),
                    Race = mobileData.Race,
                    BodyForms = ConvertBodyForms(mobileData.Form),
                    BodyParts = ConvertBodyParts(mobileData.Parts),
                    Group = mobileData.Group,
                    //
                    PetBlueprintIds = petBlueprintIds,
                    ProfitBuy = mobileData.Shop?.ProfitBuy ?? 100,
                    ProfitSell = mobileData.Shop?.ProfitSell ?? 100,
                    OpenHour = mobileData.Shop?.OpenHour ?? 0,
                    CloseHour = mobileData.Shop?.CloseHour ?? 23,
                };
            }
        }

        if (mobileData.Shop == null)
            return new CharacterNormalBlueprint
            {
                Id = mobileData.VNum,
                Name = mobileData.Name,
                Description = mobileData.Description,
                Level = mobileData.Level,
                LongDescription = mobileData.LongDescr,
                ShortDescription = mobileData.ShortDescr,
                Sex = ConvertSex(mobileData),
                Size = ConvertSize(mobileData),
                Wealth = mobileData.Wealth,
                Alignment = mobileData.Alignment,
                DamageNoun = damageNoun,
                DamageType = schoolType,
                DamageDiceCount = mobileData.Damage[0],
                DamageDiceValue = mobileData.Damage[1],
                DamageDiceBonus = mobileData.Damage[2],
                HitPointDiceCount = mobileData.Hit[0],
                HitPointDiceValue = mobileData.Hit[1],
                HitPointDiceBonus = mobileData.Hit[2],
                ManaDiceCount = mobileData.Mana[0],
                ManaDiceValue = mobileData.Mana[1],
                ManaDiceBonus = mobileData.Mana[2],
                HitRollBonus = mobileData.HitRoll,
                ArmorPierce = mobileData.Armor[0],
                ArmorBash = mobileData.Armor[1],
                ArmorSlash = mobileData.Armor[2],
                ArmorExotic = mobileData.Armor[3],
                CharacterFlags = characterFlags,
                ActFlags = ConvertActFlags(mobileData.Act),
                OffensiveFlags = offensiveFlags,
                AssistFlags = assistFlags,
                Immunities = ConvertIRV(mobileData.ImmFlags),
                Resistances = ConvertIRV(mobileData.ResFlags),
                Vulnerabilities = ConvertIRV(mobileData.VulnFlags),
                ShieldFlags = shieldFlags,
                StartPosition = ConvertPosition(mobileData, mobileData.StartPos),
                DefaultPosition = ConvertPosition(mobileData, mobileData.DefaultPos),
                Race = mobileData.Race,
                BodyForms = ConvertBodyForms(mobileData.Form),
                BodyParts = ConvertBodyParts(mobileData.Parts),
                SpecialBehavior = mobileData.Special,
                Group = mobileData.Group,
            };
        else
        {
            return new CharacterShopBlueprint
            {
                Id = mobileData.VNum,
                Name = mobileData.Name,
                Description = mobileData.Description,
                Level = mobileData.Level,
                LongDescription = mobileData.LongDescr,
                ShortDescription = mobileData.ShortDescr,
                Sex = ConvertSex(mobileData),
                Size = ConvertSize(mobileData),
                Wealth = mobileData.Wealth,
                Alignment = mobileData.Alignment,
                DamageNoun = damageNoun,
                DamageType = schoolType,
                DamageDiceCount = mobileData.Damage[0],
                DamageDiceValue = mobileData.Damage[1],
                DamageDiceBonus = mobileData.Damage[2],
                HitPointDiceCount = mobileData.Hit[0],
                HitPointDiceValue = mobileData.Hit[1],
                HitPointDiceBonus = mobileData.Hit[2],
                ManaDiceCount = mobileData.Mana[0],
                ManaDiceValue = mobileData.Mana[1],
                ManaDiceBonus = mobileData.Mana[2],
                HitRollBonus = mobileData.HitRoll,
                ArmorPierce = mobileData.Armor[0],
                ArmorBash = mobileData.Armor[1],
                ArmorSlash = mobileData.Armor[2],
                ArmorExotic = mobileData.Armor[3],
                CharacterFlags = characterFlags,
                ActFlags = ConvertActFlags(mobileData.Act),
                OffensiveFlags = offensiveFlags,
                AssistFlags = assistFlags,
                Immunities = ConvertIRV(mobileData.ImmFlags),
                Resistances = ConvertIRV(mobileData.ResFlags),
                Vulnerabilities = ConvertIRV(mobileData.VulnFlags),
                ShieldFlags = shieldFlags,
                StartPosition = ConvertPosition(mobileData, mobileData.StartPos),
                DefaultPosition = ConvertPosition(mobileData, mobileData.DefaultPos),
                Race = mobileData.Race,
                BodyForms = ConvertBodyForms(mobileData.Form),
                BodyParts = ConvertBodyParts(mobileData.Parts),
                Group = mobileData.Group,
                //
                BuyBlueprintTypes = ConvertBuyTypes(mobileData.Shop).ToList(),
                ProfitBuy = mobileData.Shop.ProfitBuy,
                ProfitSell = mobileData.Shop.ProfitSell,
                OpenHour = mobileData.Shop.OpenHour,
                CloseHour = mobileData.Shop.CloseHour,
            };
        }
    }

    private Positions ConvertPosition(MobileData mobileData, string position)
    {
        switch (position)
        {
            case "stand": return Positions.Standing;
            case "sit": return Positions.Sitting;
            case "rest": return Positions.Resting;
            case "sleep": return Positions.Sleeping;
            default:
                Logger.LogError("Invalid position {position} for mob {vnum}", mobileData.DefaultPos, mobileData.VNum);
                return Positions.Standing;
        }
    }

    private Sex ConvertSex(MobileData mobileData)
    {
        if (mobileData.Sex.ToLower() == "female")
            return Sex.Female;
        if (mobileData.Sex.ToLower() == "male")
            return Sex.Male;
        return Sex.Neutral;
    }

    private Sizes ConvertSize(MobileData mobileData)
    {
        switch (mobileData.Size)
        {
            case "tiny": return Sizes.Tiny;
            case "small": return Sizes.Small;
            case "medium": return Sizes.Medium;
            case "large": return Sizes.Large;
            case "huge": return Sizes.Huge;
            case "giant": return Sizes.Giant;
            default:
                Logger.LogError("Invalid size {0} for mob {1}", mobileData.Size, mobileData.VNum);
                return Sizes.Medium;
        }
    }

    private IIRVFlags ConvertIRV(long value)
    {
        var flags = new IRVFlags();
        if (IsSet(value, IMM_SUMMON)) flags.Set("Summon");
        if (IsSet(value, IMM_CHARM)) flags.Set("Charm");
        if (IsSet(value, IMM_MAGIC)) flags.Set("Magic");
        if (IsSet(value, IMM_WEAPON)) flags.Set("Weapon");
        if (IsSet(value, IMM_BASH)) flags.Set("Bash");
        if (IsSet(value, IMM_PIERCE)) flags.Set("Pierce");
        if (IsSet(value, IMM_SLASH)) flags.Set("Slash");
        if (IsSet(value, IMM_FIRE)) flags.Set("Fire");
        if (IsSet(value, IMM_COLD)) flags.Set("Cold");
        if (IsSet(value, IMM_LIGHTNING)) flags.Set("Lightning");
        if (IsSet(value, IMM_ACID)) flags.Set("Acid");
        if (IsSet(value, IMM_POISON)) flags.Set("Poison");
        if (IsSet(value, IMM_NEGATIVE)) flags.Set("Negative");
        if (IsSet(value, IMM_HOLY)) flags.Set("Holy");
        if (IsSet(value, IMM_ENERGY)) flags.Set("Energy");
        if (IsSet(value, IMM_MENTAL)) flags.Set("Mental");
        if (IsSet(value, IMM_DISEASE)) flags.Set("Disease");
        if (IsSet(value, IMM_DROWNING)) flags.Set("Drowning");
        if (IsSet(value, IMM_LIGHT)) flags.Set("Light");
        if (IsSet(value, IMM_SOUND)) flags.Set("Sound");
        if (IsSet(value, IMM_WOOD)) flags.Set("Wood");
        if (IsSet(value, IMM_SILVER)) flags.Set("Silver");
        if (IsSet(value, IMM_IRON)) flags.Set("Iron");

        return flags;
    }

    private (ICharacterFlags characterFlags, IShieldFlags shieldFlags) ConvertCharacterFlags(long affectedBy)
    {
        var characterFlags = new CharacterFlags();
        var shieldFlags = new ShieldFlags();
        if (IsSet(affectedBy, AFF_BLIND)) characterFlags.Set("Blind");
        if (IsSet(affectedBy, AFF_INVISIBLE)) characterFlags.Set("Invisible");
        if (IsSet(affectedBy, AFF_DETECT_EVIL)) characterFlags.Set("DetectEvil");
        if (IsSet(affectedBy, AFF_DETECT_INVIS)) characterFlags.Set("DetectInvis");
        if (IsSet(affectedBy, AFF_DETECT_MAGIC)) characterFlags.Set("DetectMagic");
        if (IsSet(affectedBy, AFF_DETECT_HIDDEN)) characterFlags.Set("DetectHidden");
        if (IsSet(affectedBy, AFF_DETECT_GOOD)) characterFlags.Set("DetectGood");
        if (IsSet(affectedBy, AFF_SANCTUARY)) shieldFlags.Set("Sanctuary");
        if (IsSet(affectedBy, AFF_FAERIE_FIRE)) characterFlags.Set("FaerieFire");
        if (IsSet(affectedBy, AFF_INFRARED)) characterFlags.Set("Infrared");
        if (IsSet(affectedBy, AFF_CURSE)) characterFlags.Set("Curse");
        // AFF_ROOTED
        if (IsSet(affectedBy, AFF_POISON)) characterFlags.Set("Poison");
        if (IsSet(affectedBy, AFF_PROTECT_EVIL)) shieldFlags.Set("ProtectEvil");
        if (IsSet(affectedBy, AFF_PROTECT_GOOD)) shieldFlags.Set("ProtectGood");
        if (IsSet(affectedBy, AFF_SNEAK)) characterFlags.Set("Sneak");
        if (IsSet(affectedBy, AFF_HIDE)) characterFlags.Set("Hide");
        if (IsSet(affectedBy, AFF_SLEEP)) characterFlags.Set("Sleep");
        if (IsSet(affectedBy, AFF_CHARM)) characterFlags.Set("Charm");
        if (IsSet(affectedBy, AFF_FLYING)) characterFlags.Set("Flying");
        if (IsSet(affectedBy, AFF_PASS_DOOR)) characterFlags.Set("PassDoor");
        if (IsSet(affectedBy, AFF_HASTE)) characterFlags.Set("Haste");
        if (IsSet(affectedBy, AFF_CALM)) characterFlags.Set("Calm");
        if (IsSet(affectedBy, AFF_PLAGUE)) characterFlags.Set("Plague");
        if (IsSet(affectedBy, AFF_WEAKEN)) characterFlags.Set("Weaken");
        if (IsSet(affectedBy, AFF_DARK_VISION)) characterFlags.Set("DarkVision");
        if (IsSet(affectedBy, AFF_BERSERK)) characterFlags.Set("Berserk");
        if (IsSet(affectedBy, AFF_SWIM)) characterFlags.Set("Swim");
        if (IsSet(affectedBy, AFF_REGENERATION)) characterFlags.Set("Regeneration");
        if (IsSet(affectedBy, AFF_SLOW)) characterFlags.Set("Slow");

        return (characterFlags, shieldFlags);
    }

    private IActFlags ConvertActFlags(long act)
    {
        var flags = new ActFlags();
        //ACT_IS_NPC not used
        if (IsSet(act, ACT_SENTINEL)) flags.Set("Sentinel");
        if (IsSet(act, ACT_SCAVENGER)) flags.Set("Scavenger");
        if (IsSet(act, ACT_AGGRESSIVE)) flags.Set("Aggressive");
        if (IsSet(act, ACT_STAY_AREA)) flags.Set("StayArea");
        if (IsSet(act, ACT_WIMPY)) flags.Set("Wimpy");
        if (IsSet(act, ACT_PET)) flags.Set("Pet");
        if (IsSet(act, ACT_TRAIN)) flags.Set("Train");
        if (IsSet(act, ACT_PRACTICE)) flags.Set("Practice");
        if (IsSet(act, ACT_UNDEAD)) flags.Set("Undead");
        if (IsSet(act, ACT_CLERIC)) flags.Set("Cleric");
        if (IsSet(act, ACT_MAGE)) flags.Set("Mage");
        if (IsSet(act, ACT_THIEF)) flags.Set("Thief");
        if (IsSet(act, ACT_WARRIOR)) flags.Set("Warrior");
        if (IsSet(act, ACT_NOALIGN)) flags.Set("NoAlign");
        if (IsSet(act, ACT_NOPURGE)) flags.Set("NoPurge");
        if (IsSet(act, ACT_OUTDOORS)) flags.Set("Outdoors");
        if (IsSet(act, ACT_INDOORS)) flags.Set("Indoors");
        if (IsSet(act, ACT_IS_HEALER)) flags.Set("IsHealer");
        if (IsSet(act, ACT_GAIN)) flags.Set("Gain");
        if (IsSet(act, ACT_UPDATE_ALWAYS)) flags.Set("UpdateAlways");
        //ACT_IS_CHANGER
        return flags;
    }

    private (IOffensiveFlags, IAssistFlags) ConvertOffensiveFlags(long input)
    {
        var off = new OffensiveFlags();
        if (IsSet(input, OFF_AREA_ATTACK)) off.Set("AreaAttack");
        if (IsSet(input, OFF_BACKSTAB)) off.Set("Backstab");
        if (IsSet(input, OFF_BASH)) off.Set("Bash");
        if (IsSet(input, OFF_BERSERK)) off.Set("Berserk");
        if (IsSet(input, OFF_DISARM)) off.Set("Disarm");
        if (IsSet(input, OFF_DODGE)) off.Set("Dodge");
        if (IsSet(input, OFF_FADE)) off.Set("Fade");
        if (IsSet(input, OFF_FAST)) off.Set("Fast");
        if (IsSet(input, OFF_KICK)) off.Set("Kick");
        if (IsSet(input, OFF_KICK_DIRT)) off.Set("DirtKick");
        if (IsSet(input, OFF_PARRY)) off.Set("Parry");
        if (IsSet(input, OFF_RESCUE)) off.Set("Rescue");
        if (IsSet(input, OFF_TAIL)) off.Set("Tail");
        if (IsSet(input, OFF_TRIP)) off.Set("Trip");
        if (IsSet(input, OFF_CRUSH)) off.Set("Crush");

        var assist = new AssistFlags();
        if (IsSet(input, ASSIST_ALL)) assist.Set("All");
        if (IsSet(input, ASSIST_ALIGN)) assist.Set("Align");
        if (IsSet(input, ASSIST_RACE)) assist.Set("Race");
        if (IsSet(input, ASSIST_PLAYERS)) assist.Set("Players");
        if (IsSet(input, ASSIST_GUARD)) assist.Set("Guard");
        if (IsSet(input, ASSIST_VNUM)) assist.Set("Vnum");

        return (off, assist);
    }

    private IEnumerable<Type> ConvertBuyTypes(ShopData shopData)
    {
        foreach (var buyType in shopData.BuyType)
        {
            switch (buyType)
            {
                case 0:
                    break;
                case ITEM_LIGHT: yield return typeof(ItemLightBlueprint); break;
                case ITEM_SCROLL: yield return typeof(ItemScrollBlueprint); break;
                case ITEM_WAND: yield return typeof(ItemWandBlueprint); break;
                case ITEM_STAFF: yield return typeof(ItemStaffBlueprint); break;
                case ITEM_WEAPON: yield return typeof(ItemWeaponBlueprint); break;
                case ITEM_TREASURE: yield return typeof(ItemTreasureBlueprint); break;
                case ITEM_ARMOR: yield return typeof(ItemArmorBlueprint); break;
                case ITEM_POTION: yield return typeof(ItemPotionBlueprint); break;
                case ITEM_CLOTHING: yield return typeof(ItemClothingBlueprint); break;
                case ITEM_FURNITURE: yield return typeof(ItemFurnitureBlueprint); break;
                case ITEM_TRASH: yield return typeof(ItemTrashBlueprint); break;
                case ITEM_CONTAINER: yield return typeof(ItemContainerBlueprint); break;
                case ITEM_DRINK_CON: yield return typeof(ItemDrinkContainerBlueprint); break;
                case ITEM_KEY: yield return typeof(ItemKeyBlueprint); break;
                case ITEM_FOOD: yield return typeof(ItemFoodBlueprint); break;
                case ITEM_MONEY: yield return typeof(ItemMoneyBlueprint); break;
                case ITEM_BOAT: yield return typeof(ItemBoatBlueprint); break;
                case ITEM_CORPSE_NPC:
                case ITEM_CORPSE_PC: yield return typeof(ItemLightBlueprint); break;
                case ITEM_FOUNTAIN: yield return typeof(ItemFountainBlueprint); break;
                case ITEM_PILL: yield return typeof(ItemPillBlueprint); break;
                case ITEM_PROTECT:
                    Logger.LogWarning("Invalid buy type {buyType} for mob {keeper}", buyType, shopData.Keeper);
                    break;
                case ITEM_MAP: yield return typeof(ItemMapBlueprint); break;
                case ITEM_PORTAL: yield return typeof(ItemPortalBlueprint); break;
                case ITEM_WARP_STONE: yield return typeof(ItemWarpStoneBlueprint); break;
                case ITEM_ROOM_KEY:
                    Logger.LogWarning("Invalid buy type {buyType} for mob {keeper}", buyType, shopData.Keeper);
                    break;
                case ITEM_GEM: yield return typeof(ItemGemBlueprint); break;
                case ITEM_JEWELRY: yield return typeof(ItemJewelryBlueprint); break;
                case ITEM_JUKEBOX: yield return typeof(ItemJukeboxBlueprint); break;
                default:
                    Logger.LogWarning("Invalid buy type {buyType} for mob {keeper}", buyType, shopData.Keeper);
                    break;
            }
        }
    }

    private IBodyForms ConvertBodyForms(long input)
    {
        var forms = new BodyForms();
        if (IsSet(input, FORM_EDIBLE)) forms.Set("Edible");
        if (IsSet(input, FORM_POISON)) forms.Set("Poison");
        if (IsSet(input, FORM_MAGICAL)) forms.Set("Magical");
        if (IsSet(input, FORM_INSTANT_DECAY)) forms.Set("InstantDecay");
        if (IsSet(input, FORM_OTHER)) forms.Set("Other");
        if (IsSet(input, FORM_ANIMAL)) forms.Set("Animal");
        if (IsSet(input, FORM_SENTIENT)) forms.Set("Sentient");
        if (IsSet(input, FORM_UNDEAD)) forms.Set("Undead");
        if (IsSet(input, FORM_CONSTRUCT)) forms.Set("Construct");
        if (IsSet(input, FORM_MIST)) forms.Set("Mist");
        if (IsSet(input, FORM_INTANGIBLE)) forms.Set("Intangible");
        if (IsSet(input, FORM_BIPED)) forms.Set("Biped");
        if (IsSet(input, FORM_CENTAUR)) forms.Set("Centaur");
        if (IsSet(input, FORM_INSECT)) forms.Set("Insect");
        if (IsSet(input, FORM_SPIDER)) forms.Set("Spider");
        if (IsSet(input, FORM_CRUSTACEAN)) forms.Set("Crustacean");
        if (IsSet(input, FORM_WORM)) forms.Set("Worm");
        if (IsSet(input, FORM_BLOB)) forms.Set("Blob");
        if (IsSet(input, FORM_MAMMAL)) forms.Set("Mammal");
        if (IsSet(input, FORM_BIRD)) forms.Set("Bird");
        if (IsSet(input, FORM_REPTILE)) forms.Set("Reptile");
        if (IsSet(input, FORM_SNAKE)) forms.Set("Snake");
        if (IsSet(input, FORM_DRAGON)) forms.Set("Dragon");
        if (IsSet(input, FORM_AMPHIBIAN)) forms.Set("Amphibian");
        if (IsSet(input, FORM_FISH)) forms.Set("Fish");
        if (IsSet(input, FORM_COLD_BLOOD)) forms.Set("ColdBlood");

        return forms;
    }

    private IBodyParts ConvertBodyParts(long input)
    {
        var parts = new BodyParts();
        if (IsSet(input, PART_HEAD)) parts.Set("Head");
        if (IsSet(input, PART_ARMS)) parts.Set("Arms");
        if (IsSet(input, PART_LEGS)) parts.Set("Legs");
        if (IsSet(input, PART_HEART)) parts.Set("Heart");
        if (IsSet(input, PART_BRAINS)) parts.Set("Brains");
        if (IsSet(input, PART_GUTS)) parts.Set("Guts");
        if (IsSet(input, PART_HANDS)) parts.Set("Hands");
        if (IsSet(input, PART_FEET)) parts.Set("Feet");
        if (IsSet(input, PART_FINGERS)) parts.Set("Fingers");
        if (IsSet(input, PART_EAR)) parts.Set("Ear");
        if (IsSet(input, PART_EYE)) parts.Set("Eye");
        if (IsSet(input, PART_LONG_TONGUE)) parts.Set("LongTongue");
        if (IsSet(input, PART_EYESTALKS)) parts.Set("Eyestalks");
        if (IsSet(input, PART_TENTACLES)) parts.Set("Tentacles");
        if (IsSet(input, PART_FINS)) parts.Set("Fins");
        if (IsSet(input, PART_WINGS)) parts.Set("Wings");
        if (IsSet(input, PART_TAIL)) parts.Set("Tail");
        if (IsSet(input, PART_CLAWS)) parts.Set("Claws");
        if (IsSet(input, PART_FANGS)) parts.Set("Fangs");
        if (IsSet(input, PART_HORNS)) parts.Set("Horns");
        if (IsSet(input, PART_SCALES)) parts.Set("Scales");
        if (IsSet(input, PART_TUSKS)) parts.Set("Tusks");

        return parts;
    }

    // Immunites, Resistances, Vulnerabilities
    private const long IMM_SUMMON = RomLoader.A;
    private const long IMM_CHARM = RomLoader.B;
    private const long IMM_MAGIC = RomLoader.C;
    private const long IMM_WEAPON = RomLoader.D;
    private const long IMM_BASH = RomLoader.E;
    private const long IMM_PIERCE = RomLoader.F;
    private const long IMM_SLASH = RomLoader.G;
    private const long IMM_FIRE = RomLoader.H;
    private const long IMM_COLD = RomLoader.I;
    private const long IMM_LIGHTNING = RomLoader.J;
    private const long IMM_ACID = RomLoader.K;
    private const long IMM_POISON = RomLoader.L;
    private const long IMM_NEGATIVE = RomLoader.M;
    private const long IMM_HOLY = RomLoader.N;
    private const long IMM_ENERGY = RomLoader.O;
    private const long IMM_MENTAL = RomLoader.P;
    private const long IMM_DISEASE = RomLoader.Q;
    private const long IMM_DROWNING = RomLoader.R;
    private const long IMM_LIGHT = RomLoader.S;
    private const long IMM_SOUND = RomLoader.T;
    private const long IMM_WOOD = RomLoader.X;
    private const long IMM_SILVER = RomLoader.Y;
    private const long IMM_IRON = RomLoader.Z;

    // Affected by
    private const long AFF_BLIND = RomLoader.A;
    private const long AFF_INVISIBLE = RomLoader.B;
    private const long AFF_DETECT_EVIL = RomLoader.C;
    private const long AFF_DETECT_INVIS = RomLoader.D;
    private const long AFF_DETECT_MAGIC = RomLoader.E;
    private const long AFF_DETECT_HIDDEN = RomLoader.F;
    private const long AFF_DETECT_GOOD = RomLoader.G;
    private const long AFF_SANCTUARY = RomLoader.H;
    private const long AFF_FAERIE_FIRE = RomLoader.I;
    private const long AFF_INFRARED = RomLoader.J;
    private const long AFF_CURSE = RomLoader.K;
    private const long AFF_UNUSED_FLAG = RomLoader.L;
    private const long AFF_POISON = RomLoader.M;
    private const long AFF_PROTECT_EVIL = RomLoader.N;
    private const long AFF_PROTECT_GOOD = RomLoader.O;
    private const long AFF_SNEAK = RomLoader.P;
    private const long AFF_HIDE = RomLoader.Q;
    private const long AFF_SLEEP = RomLoader.R;
    private const long AFF_CHARM = RomLoader.S;
    private const long AFF_FLYING = RomLoader.T;
    private const long AFF_PASS_DOOR = RomLoader.U;
    private const long AFF_HASTE = RomLoader.V;
    private const long AFF_CALM = RomLoader.W;
    private const long AFF_PLAGUE = RomLoader.X;
    private const long AFF_WEAKEN = RomLoader.Y;
    private const long AFF_DARK_VISION = RomLoader.Z;
    private const long AFF_BERSERK = RomLoader.aa;
    private const long AFF_SWIM = RomLoader.bb;
    private const long AFF_REGENERATION = RomLoader.cc;
    private const long AFF_SLOW = RomLoader.dd;

    // Act flags
    private const long ACT_IS_NPC = RomLoader.A;
    private const long ACT_SENTINEL = RomLoader.B;
    private const long ACT_SCAVENGER = RomLoader.C;
    private const long ACT_AGGRESSIVE = RomLoader.F;
    private const long ACT_STAY_AREA = RomLoader.G;
    private const long ACT_WIMPY = RomLoader.H;
    private const long ACT_PET = RomLoader.I;
    private const long ACT_TRAIN = RomLoader.J;
    private const long ACT_PRACTICE = RomLoader.K;
    private const long ACT_UNDEAD = RomLoader.O;
    private const long ACT_CLERIC = RomLoader.Q;
    private const long ACT_MAGE = RomLoader.R;
    private const long ACT_THIEF = RomLoader.S;
    private const long ACT_WARRIOR = RomLoader.T;
    private const long ACT_NOALIGN = RomLoader.U;
    private const long ACT_NOPURGE = RomLoader.V;
    private const long ACT_OUTDOORS = RomLoader.W;
    private const long ACT_INDOORS = RomLoader.Y;
    private const long ACT_IS_HEALER = RomLoader.aa;
    private const long ACT_GAIN = RomLoader.bb;
    private const long ACT_UPDATE_ALWAYS = RomLoader.cc;
    private const long ACT_IS_CHANGER = RomLoader.dd;

    // Offensive flags
    private const long OFF_AREA_ATTACK = RomLoader.A;
    private const long OFF_BACKSTAB = RomLoader.B;
    private const long OFF_BASH = RomLoader.C;
    private const long OFF_BERSERK = RomLoader.D;
    private const long OFF_DISARM = RomLoader.E;
    private const long OFF_DODGE = RomLoader.F;
    private const long OFF_FADE = RomLoader.G;
    private const long OFF_FAST = RomLoader.H;
    private const long OFF_KICK = RomLoader.I;
    private const long OFF_KICK_DIRT = RomLoader.J;
    private const long OFF_PARRY = RomLoader.K;
    private const long OFF_RESCUE = RomLoader.L;
    private const long OFF_TAIL = RomLoader.M;
    private const long OFF_TRIP = RomLoader.N;
    private const long OFF_CRUSH = RomLoader.O;
    private const long ASSIST_ALL = RomLoader.P;
    private const long ASSIST_ALIGN = RomLoader.Q;
    private const long ASSIST_RACE = RomLoader.R;
    private const long ASSIST_PLAYERS = RomLoader.S;
    private const long ASSIST_GUARD = RomLoader.T;
    private const long ASSIST_VNUM = RomLoader.U;

    // Item types
    private const int ITEM_LIGHT = 1;
    private const int ITEM_SCROLL = 2;
    private const int ITEM_WAND = 3;
    private const int ITEM_STAFF = 4;
    private const int ITEM_WEAPON = 5;
    private const int ITEM_TREASURE = 8;
    private const int ITEM_ARMOR = 9;
    private const int ITEM_POTION = 10;
    private const int ITEM_CLOTHING = 11;
    private const int ITEM_FURNITURE = 12;
    private const int ITEM_TRASH = 13;
    private const int ITEM_CONTAINER = 15;
    private const int ITEM_DRINK_CON = 17;
    private const int ITEM_KEY = 18;
    private const int ITEM_FOOD = 19;
    private const int ITEM_MONEY = 20;
    private const int ITEM_BOAT = 22;
    private const int ITEM_CORPSE_NPC = 23;
    private const int ITEM_CORPSE_PC = 24;
    private const int ITEM_FOUNTAIN = 25;
    private const int ITEM_PILL = 26;
    private const int ITEM_PROTECT = 27;
    private const int ITEM_MAP = 28;
    private const int ITEM_PORTAL = 29;
    private const int ITEM_WARP_STONE = 30;
    private const int ITEM_ROOM_KEY = 31;
    private const int ITEM_GEM = 32;
    private const int ITEM_JEWELRY = 33;
    private const int ITEM_JUKEBOX = 34;

    // body form
    private const long FORM_EDIBLE = RomLoader.A;
    private const long FORM_POISON = RomLoader.B;
    private const long FORM_MAGICAL = RomLoader.C;
    private const long FORM_INSTANT_DECAY = RomLoader.D;
    private const long FORM_OTHER = RomLoader.E;  /* defined by material bit */
    // actual form
    private const long FORM_ANIMAL = RomLoader.G;
    private const long FORM_SENTIENT = RomLoader.H;
    private const long FORM_UNDEAD = RomLoader.I;
    private const long FORM_CONSTRUCT = RomLoader.J;
    private const long FORM_MIST = RomLoader.K;
    private const long FORM_INTANGIBLE = RomLoader.L;
    private const long FORM_BIPED = RomLoader.M;
    private const long FORM_CENTAUR = RomLoader.N;
    private const long FORM_INSECT = RomLoader.O;
    private const long FORM_SPIDER = RomLoader.P;
    private const long FORM_CRUSTACEAN = RomLoader.Q;
    private const long FORM_WORM = RomLoader.R;
    private const long FORM_BLOB = RomLoader.S;
    private const long FORM_MAMMAL = RomLoader.V;
    private const long FORM_BIRD = RomLoader.W;
    private const long FORM_REPTILE = RomLoader.X;
    private const long FORM_SNAKE = RomLoader.Y;
    private const long FORM_DRAGON = RomLoader.Z;
    private const long FORM_AMPHIBIAN = RomLoader.aa;
    private const long FORM_FISH = RomLoader.bb;
    private const long FORM_COLD_BLOOD = RomLoader.cc;

    // body parts
    private const long PART_HEAD = RomLoader.A;
    private const long PART_ARMS = RomLoader.B;
    private const long PART_LEGS = RomLoader.C;
    private const long PART_HEART = RomLoader.D;
    private const long PART_BRAINS = RomLoader.E;
    private const long PART_GUTS = RomLoader.F;
    private const long PART_HANDS = RomLoader.G;
    private const long PART_FEET = RomLoader.H;
    private const long PART_FINGERS = RomLoader.I;
    private const long PART_EAR = RomLoader.J;
    private const long PART_EYE = RomLoader.K;
    private const long PART_LONG_TONGUE = RomLoader.L;
    private const long PART_EYESTALKS = RomLoader.M;
    private const long PART_TENTACLES = RomLoader.N;
    private const long PART_FINS = RomLoader.O;
    private const long PART_WINGS = RomLoader.P;
    private const long PART_TAIL = RomLoader.Q;
    // for combat
    private const long PART_CLAWS = RomLoader.U;
    private const long PART_FANGS = RomLoader.V;
    private const long PART_HORNS = RomLoader.W;
    private const long PART_SCALES = RomLoader.X;
    private const long PART_TUSKS = RomLoader.Y;

    #endregion

    private SchoolTypes ConvertDamageType(int damageType, string errorMsg)
    {
        switch (damageType)
        {
            case DAM_NONE: return SchoolTypes.None;
            case DAM_BASH: return SchoolTypes.Bash;
            case DAM_PIERCE: return SchoolTypes.Pierce;
            case DAM_SLASH: return SchoolTypes.Slash;
            case DAM_FIRE: return SchoolTypes.Fire;
            case DAM_COLD: return SchoolTypes.Cold;
            case DAM_LIGHTNING: return SchoolTypes.Lightning;
            case DAM_ACID: return SchoolTypes.Acid;
            case DAM_POISON: return SchoolTypes.Poison;
            case DAM_NEGATIVE: return SchoolTypes.Negative;
            case DAM_HOLY: return SchoolTypes.Holy;
            case DAM_ENERGY: return SchoolTypes.Energy;
            case DAM_MENTAL: return SchoolTypes.Mental;
            case DAM_DISEASE: return SchoolTypes.Disease;
            case DAM_DROWNING: return SchoolTypes.Drowning;
            case DAM_LIGHT: return SchoolTypes.Light;
            case DAM_OTHER: return SchoolTypes.Other;
            case DAM_HARM: return SchoolTypes.Harm;
            case DAM_CHARM: return SchoolTypes.Charm;
            case DAM_SOUND: return SchoolTypes.Sound;
        }

        Logger.LogWarning("Unknown damage type {type} for {msg}", damageType, errorMsg);
        return SchoolTypes.None;
    }

    // damage types
    private const int DAM_NONE = 0;
    private const int DAM_BASH = 1;
    private const int DAM_PIERCE = 2;
    private const int DAM_SLASH = 3;
    private const int DAM_FIRE = 4;
    private const int DAM_COLD = 5;
    private const int DAM_LIGHTNING = 6;
    private const int DAM_ACID = 7;
    private const int DAM_POISON = 8;
    private const int DAM_NEGATIVE = 9;
    private const int DAM_HOLY = 10;
    private const int DAM_ENERGY = 11;
    private const int DAM_MENTAL = 12;
    private const int DAM_DISEASE = 13;
    private const int DAM_DROWNING = 14;
    private const int DAM_LIGHT = 15;
    private const int DAM_OTHER = 16;
    private const int DAM_HARM = 17;
    private const int DAM_CHARM = 18;
    private const int DAM_SOUND = 19;

    // attack table
    private static readonly (string name, string noun, int damType)[] AttackTable =
    {
        ("none", "hit", DAM_NONE), /*  0 */ // was -1 in ROm2.4
        ("slice", "slice", DAM_SLASH),
        ("stab", "stab", DAM_PIERCE),
        ("slash", "slash", DAM_SLASH),
        ("whip", "whip", DAM_SLASH),
        ("claw", "claw", DAM_SLASH), /*  5 */
        ("blast", "blast", DAM_BASH),
        ("pound", "pound", DAM_BASH),
        ("crush", "crush", DAM_BASH),
        ("grep", "grep", DAM_SLASH),
        ("bite", "bite", DAM_PIERCE), /* 10 */
        ("pierce", "pierce", DAM_PIERCE),
        ("suction", "suction", DAM_BASH),
        ("beating", "beating", DAM_BASH),
        ("digestion", "digestion", DAM_ACID),
        ("charge", "charge", DAM_BASH), /* 15 */
        ("slap", "slap", DAM_BASH),
        ("punch", "punch", DAM_BASH),
        ("wrath", "wrath", DAM_ENERGY),
        ("magic", "magic", DAM_ENERGY),
        ("divine", "divine power", DAM_HOLY), /* 20 */
        ("cleave", "cleave", DAM_SLASH),
        ("scratch", "scratch", DAM_PIERCE),
        ("peck", "peck", DAM_PIERCE),
        ("peckb", "peck", DAM_BASH),
        ("chop", "chop", DAM_SLASH), /* 25 */
        ("sting", "sting", DAM_PIERCE),
        ("smash", "smash", DAM_BASH),
        ("shbite", "shocking bite", DAM_LIGHTNING),
        ("flbite", "flaming bite", DAM_FIRE),
        ("frbite", "freezing bite", DAM_COLD), /* 30 */
        ("acbite", "acidic bite", DAM_ACID),
        ("chomp", "chomp", DAM_PIERCE),
        ("drain", "life drain", DAM_NEGATIVE),
        ("thrust", "thrust", DAM_PIERCE),
        ("slime", "slime", DAM_ACID),
        ("shock", "shock", DAM_LIGHTNING),
        ("thwack", "thwack", DAM_BASH),
        ("flame", "flame", DAM_FIRE),
        ("chill", "chill", DAM_COLD),
    };

    //
    private bool IsSet(long input, long bit) => (input & bit) == bit;

    //
    private void RaiseConvertException(string format, params object[] parameters)
    {
        var message = string.Format(format, parameters);
        Logger.LogError(message);
        throw new RomConvertException(message);
    }

    //
    private string RemoveCommentIfAny(string filename)
    {
        var index = filename.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
        if (index >= 0)
            return filename.Remove(index).Trim();
        return filename;
    }
}
