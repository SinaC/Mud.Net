using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using System.Diagnostics;

namespace Mud.Importer.Rot;

[Export, Shared]
public class RotImporter
{
    private ILogger<RotImporter> Logger { get; }
    private IServiceProvider ServiceProvider { get; }

    private readonly List<AreaBlueprint> _areaBlueprints = [];
    private readonly List<RoomBlueprint> _roomBlueprints = [];
    private readonly List<ItemBlueprintBase> _itemBlueprints = [];
    private readonly List<CharacterBlueprintBase> _characterBlueprints = [];

    public IReadOnlyCollection<AreaBlueprint> Areas => _areaBlueprints.AsReadOnly();
    public IReadOnlyCollection<RoomBlueprint> Rooms => _roomBlueprints.AsReadOnly();
    public IReadOnlyCollection<ItemBlueprintBase> Items => _itemBlueprints.AsReadOnly();
    public IReadOnlyCollection<CharacterBlueprintBase> Characters => _characterBlueprints.AsReadOnly();

    public RotImporter(ILogger<RotImporter> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public void ImportByList(string path, string areaLst)
    {
        RotLoader loader = new(ServiceProvider.GetRequiredService<ILogger<RotLoader>>()); // TODO: register RotLoader
        string[] areaFilenames = File.ReadAllLines(Path.Combine(path, areaLst));
        foreach (string areaFilename in areaFilenames)
        {
            if (areaFilename.Contains("$"))
                break;
            if (areaFilename.StartsWith("-"))
                Logger.LogInformation("Skipping {0}", areaFilename);
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
        RotLoader loader = new(ServiceProvider.GetRequiredService<ILogger<RotLoader>>()); // TODO: register RotLoader
        foreach (string filename in filenames)
        {
            string fullName = Path.Combine(path, filename);
            loader.Load(fullName);
            loader.Parse();
        }

        Convert(loader);
    }

    public void Import(string path, IEnumerable<string> filenames)
    {
        RotLoader loader = new(ServiceProvider.GetRequiredService<ILogger<RotLoader>>()); // TODO: register RotLoader
        foreach (string filename in filenames)
        {
            string fullName = Path.Combine(path, filename);
            loader.Load(fullName);
            loader.Parse();
        }

        Convert(loader);
    }

    private void Convert(RotLoader loader)
    {
        foreach (var areaData in loader.Areas)
        {
            AreaBlueprint areaBlueprint = ConvertArea(areaData);
            if (areaBlueprint != null)
                _areaBlueprints.Add(areaBlueprint);
        }

        foreach (var roomData in loader.Rooms)
        {
            RoomBlueprint roomBlueprint = ConvertRoom(roomData);
            if (roomBlueprint != null)
                _roomBlueprints.Add(roomBlueprint);
        }

        foreach (var objectData in loader.Objects)
        {
            ItemBlueprintBase itemBlueprint = ConvertObject(objectData);
            if (itemBlueprint != null)
                _itemBlueprints.Add(itemBlueprint);
        }

        foreach (var mobileData in loader.Mobiles)
        {
            CharacterBlueprintBase characterBlueprint = ConvertMobile(mobileData);
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
            Security = areaData.Security
        };
    }

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
            RoomFlags = ConvertRoomFlags(roomData.RoomFlags),
            SectorType = ConvertSectorType(roomData.SectorType),
            HealRate = roomData.HealRate,
            ResourceRate = roomData.ManaRate,
            MaxSize = null,
            Exits = ConvertExits(roomData),
            Resets = ConvertResets(roomData).ToList()
        };
        // Clan, GUid, Race, Transfer, Owner, HealNeg not used
    }

    private ExitBlueprint[] ConvertExits(RoomData roomData)
    {
        ExitBlueprint[] blueprints = new ExitBlueprint[RoomData.MaxDir];
        for (int i = 0; i < RoomData.MaxDir; i++)
        {
            ExitData exit = roomData.Exits[i];
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

    private ExitFlags ConvertExitFlags(long input)
    {
        ExitFlags flags = ExitFlags.None;
        if (IsSet(input, EX_ISDOOR)) flags |= ExitFlags.Door;
        if (IsSet(input, EX_CLOSED)) flags |= ExitFlags.Closed;
        if (IsSet(input, EX_LOCKED)) flags |= ExitFlags.Locked;
        if (IsSet(input, EX_PICKPROOF)) flags |= ExitFlags.PickProof;
        if (IsSet(input, EX_NOPASS)) flags |= ExitFlags.NoPass;
        if (IsSet(input, EX_EASY)) flags |= ExitFlags.Easy;
        if (IsSet(input, EX_HARD)) flags |= ExitFlags.Hard;
        if (IsSet(input, EX_INVIS)) flags |= ExitFlags.Hidden;
        // EX_FLY, EX_SWIM, EX_MAGIC
        return flags;
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
        //TODO: ROOM_PET_SHOP
        if (IsSet(input, ROOM_NO_RECALL)) flags.Set("NoRecall");
        if (IsSet(input, ROOM_IMP_ONLY)) flags.Set("ImpOnly");
        if (IsSet(input, ROOM_GODS_ONLY)) flags.Set("GodsOnly");
        //TODO: ROOM_HEROES_ONLY
        if (IsSet(input, ROOM_NEWBIES_ONLY)) flags.Set("NewbiesOnly");
        if (IsSet(input, ROOM_LAW)) flags.Set("Law");
        if (IsSet(input, ROOM_NOWHERE)) flags.Set("NoWhere");
        //if (IsSet(input, ROOM_CLAN_ENT)) flags |= 
        //if (IsSet(input, ROOM_ARENA)) flags |= 
        //if (IsSet(input, ROOM_LOCKED)) flags |= 

        return flags;
    }

    private SectorTypes ConvertSectorType(int sector)
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

    // Exit flags
    private const long EX_ISDOOR = RotLoader.A;
    private const long EX_CLOSED = RotLoader.B;
    private const long EX_LOCKED = RotLoader.C;
    private const long EX_PICKPROOF = RotLoader.F;
    private const long EX_NOPASS = RotLoader.G;
    private const long EX_EASY = RotLoader.H;
    private const long EX_HARD = RotLoader.I;
    private const long EX_INFURIATING = RotLoader.J;
    private const long EX_NOCLOSE = RotLoader.K;
    private const long EX_NOLOCK = RotLoader.L;
    private const long EX_INVIS = RotLoader.M;
    private const long EX_FLY = RotLoader.N;
    private const long EX_SWIM = RotLoader.O;
    private const long EX_MAGIC = RotLoader.P;

    // Room flags
    private const long ROOM_DARK = RotLoader.A;
    private const long ROOM_NO_MOB = RotLoader.C;
    private const long ROOM_INDOORS = RotLoader.D;
    private const long ROOM_PRIVATE = RotLoader.J;
    private const long ROOM_SAFE = RotLoader.K;
    private const long ROOM_SOLITARY = RotLoader.L;
    private const long ROOM_PET_SHOP = RotLoader.M;
    private const long ROOM_NO_RECALL = RotLoader.N;
    private const long ROOM_IMP_ONLY = RotLoader.O;
    private const long ROOM_GODS_ONLY = RotLoader.P;
    private const long ROOM_HEROES_ONLY = RotLoader.Q;
    private const long ROOM_NEWBIES_ONLY = RotLoader.R;
    private const long ROOM_LAW = RotLoader.S;
    private const long ROOM_NOWHERE = RotLoader.T;
    private const long ROOM_CLAN_ENT = RotLoader.U;
    private const long ROOM_ARENA = RotLoader.V;
    private const long ROOM_LOCKED = RotLoader.X;

    // Sector
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
                        Logger.LogWarning($"Reset M arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
                    if (reset.Arg4 == 0)
                        Logger.LogWarning($"Reset M arg4 (local limit) is 0 for room id '{roomData.VNum}'.");
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
                    yield return new ItemInRoomReset
                    {
                        RoomId = roomData.VNum,
                        ItemId = reset.Arg1,
                        GlobalLimit = -1, // no global limit
                        LocalLimit = 1, //reset.Arg4, in db.c/reset_room once we find the item we don't load another one
                    };
                    break;
                case 'P':
                    if (reset.Arg2 == 0)
                        Logger.LogWarning($"Reset P arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
                    if (reset.Arg4 == 0)
                        Logger.LogWarning($"Reset P arg4 (local limit) is 0 for room id '{roomData.VNum}'.");
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
                        Logger.LogWarning($"Reset G arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
                    yield return new ItemInCharacterReset
                    {
                        RoomId = roomData.VNum,
                        ItemId = reset.Arg1,
                        GlobalLimit = reset.Arg2,
                    };
                    break;
                case 'E':
                    if (reset.Arg2 == 0)
                        Logger.LogWarning($"Reset E arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
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
                    Logger.LogError("Unknown Reset {0} for room {1}", reset.Command, roomData.VNum);
                    break;
            }
        }
    }

    private EquipmentSlots ConvertResetDataWearLocation(int resetDataWearLocation)
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
            case WEAR_SECONDARY: return EquipmentSlots.OffHand;
                // WEAR_FACE
        }

        return EquipmentSlots.None;
    }

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
    private const int WEAR_SECONDARY = 19;
    private const int WEAR_FACE = 20;

    #endregion

    #region Object

    private ItemBlueprintBase ConvertObject(ObjectData objectData)
    {
        if (_itemBlueprints.Any(x => x.Id == objectData.VNum))
            RaiseConvertException("Duplicate object Id {0}", objectData.VNum);
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
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    DurationHours = ConvertObjectToInt32(objectData.Values[2]),
                };
            case "scroll":
                return new ItemScrollBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
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
                        Cost = System.Convert.ToInt32(objectData.Cost),
                        Level = objectData.Level,
                        Weight = objectData.Weight,
                        WearLocation = ConvertWearLocation(objectData),
                        ItemFlags = ConvertExtraFlags(objectData),
                        NoTake = IsNoTake(objectData),
                        Armor = objectData.Values.Take(4).Sum(ConvertObjectToInt32), // TODO
                    };
                else
                    return new ItemArmorBlueprint
                    {
                        Id = objectData.VNum,
                        Name = objectData.Name,
                        ShortDescription = objectData.ShortDescr,
                        Description = objectData.Description,
                        ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
                        Cost = System.Convert.ToInt32(objectData.Cost),
                        Level = objectData.Level,
                        Weight = objectData.Weight,
                        WearLocation = ConvertWearLocation(objectData),
                        ItemFlags = ConvertExtraFlags(objectData),
                        NoTake = IsNoTake(objectData),
                        Pierce = ConvertObjectToInt32(objectData.Values[0]),
                        Bash = ConvertObjectToInt32(objectData.Values[1]),
                        Slash = ConvertObjectToInt32(objectData.Values[2]),
                        Exotic = ConvertObjectToInt32(objectData.Values[3]),
                    };
            case "potion":
                return new ItemPotionBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
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
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    MaxPeople = ConvertObjectToInt32(objectData.Values[0]),
                    MaxWeight = ConvertObjectToInt32(objectData.Values[1]),
                    FurnitureActions = ConvertFurnitureActions(objectData),
                    FurniturePlacePreposition = ConvertFurniturePreposition(objectData),
                    HealBonus = ConvertObjectToInt32(objectData.Values[3]),
                    ResourceBonus = ConvertObjectToInt32(objectData.Values[4]),
                };
            case "trash":
                return new ItemTrashBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
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
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                    FullHours = ConvertObjectToInt32(objectData.Values[0]),
                    HungerHours = ConvertObjectToInt32(objectData.Values[1]),
                    IsPoisoned = ConvertObjectToInt32(objectData.Values[3]) != 0,
                };
            case "money":
                return new ItemMoneyBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
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
            //case "protect":
            case "map":
                return new ItemMapBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
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
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            //case "room_key":
            case "gem":
                return new ItemGemBlueprint
                {
                    Id = objectData.VNum,
                    Name = objectData.Name,
                    ShortDescription = objectData.ShortDescr,
                    Description = objectData.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(objectData.ExtraDescr),
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
                    Cost = System.Convert.ToInt32(objectData.Cost),
                    Level = objectData.Level,
                    Weight = objectData.Weight,
                    WearLocation = ConvertWearLocation(objectData),
                    ItemFlags = ConvertExtraFlags(objectData),
                    NoTake = IsNoTake(objectData),
                };
            //case "demon_stone":
            //case "exit":
            //case "pit":
            //case "passbook":
            //case "vehicle":
            default:
                Logger.LogWarning($"ItemBlueprint cannot be created: [{objectData.VNum}] [{objectData.ItemType}] [{objectData.WearFlags}] : {objectData.Name}");
                break;
        }
        // Material, Condition, Affects, Clan, Guild not used

        return null!;
    }

    private bool IsNoTake(ObjectData objectData) => (objectData.WearFlags & ITEM_TAKE) != ITEM_TAKE;

    private WearLocations ConvertWearLocation(ObjectData objectData) // in Rot2.4 it's a flag but in our code it a single value
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
            //set in ItemFlags ITEM_NO_SAC
            case ITEM_WEAR_FLOAT: return WearLocations.Float;
                //ITEM_WEAR_FACE
        }

        Logger.LogWarning("Unknown wear location: {0} for item {1}", objectData.WearFlags, objectData.VNum);
        return WearLocations.None;
    }

    private IItemFlags ConvertExtraFlags(ObjectData objectData)
    {
        var itemFlags = new ItemFlags();
        if (IsSet(objectData.ExtraFlags, ITEM_GLOW)) itemFlags.Set("Glowing");
        if (IsSet(objectData.ExtraFlags, ITEM_HUM)) itemFlags.Set("Humming");
        if (IsSet(objectData.ExtraFlags, ITEM_DARK)) itemFlags.Set("Dark");
        if (IsSet(objectData.ExtraFlags, ITEM_LOCK)) itemFlags.Set("Lock");
        if (IsSet(objectData.ExtraFlags, ITEM_EVIL)) itemFlags.Set("Evil");
        if (IsSet(objectData.ExtraFlags, ITEM_INVIS)) itemFlags.Set("Invis");
        if (IsSet(objectData.ExtraFlags, ITEM_MAGIC)) itemFlags.Set("Magic");
        if (IsSet(objectData.ExtraFlags, ITEM_NODROP)) itemFlags.Set("NoDrop");
        if (IsSet(objectData.ExtraFlags, ITEM_BLESS)) itemFlags.Set("Bless");
        if (IsSet(objectData.ExtraFlags, ITEM_ANTI_GOOD)) itemFlags.Set("AntiGood");
        if (IsSet(objectData.ExtraFlags, ITEM_ANTI_EVIL)) itemFlags.Set("AntiEvil");
        if (IsSet(objectData.ExtraFlags, ITEM_ANTI_NEUTRAL)) itemFlags.Set("AntiNeutral");
        if (IsSet(objectData.ExtraFlags, ITEM_NOREMOVE)) itemFlags.Set("NoRemove");
        if (IsSet(objectData.ExtraFlags, ITEM_INVENTORY)) itemFlags.Set("Inventory");
        if (IsSet(objectData.ExtraFlags, ITEM_NOPURGE)) itemFlags.Set("NoPurge");
        if (IsSet(objectData.ExtraFlags, ITEM_ROT_DEATH)) itemFlags.Set("RotDeath");
        if (IsSet(objectData.ExtraFlags, ITEM_VIS_DEATH)) itemFlags.Set("VisibleDeath");
        if (IsSet(objectData.ExtraFlags, ITEM_NOSAC)) itemFlags.Set("NoSacrifice");
        if (IsSet(objectData.ExtraFlags, ITEM_NONMETAL)) itemFlags.Set("NonMetal");
        if (IsSet(objectData.ExtraFlags, ITEM_NOLOCATE)) itemFlags.Set("NoLocate");
        if (IsSet(objectData.ExtraFlags, ITEM_MELT_DROP)) itemFlags.Set("MeltOnDrop");
        if (IsSet(objectData.ExtraFlags, ITEM_HAD_TIMER)) itemFlags.Set("HadTimer");
        if (IsSet(objectData.ExtraFlags, ITEM_SELL_EXTRACT)) itemFlags.Set("SellExtract");
        if (IsSet(objectData.ExtraFlags, ITEM_BURN_PROOF)) itemFlags.Set("BurnProof");
        if (IsSet(objectData.ExtraFlags, ITEM_NOUNCURSE)) itemFlags.Set("NoUncurse");
        //ITEM_LQUEST
        //ITEM_FORCED
        //ITEM_QUESTPOINT
        //ITEM_QUEST
        if (IsSet(objectData.WearFlags, ITEM_NO_SAC)) itemFlags.Set("NoSacrifice");

        return itemFlags;
    }

    private WeaponTypes ConvertWeaponType(ObjectData objectData)
    {
        string weaponType = (string)objectData.Values[0];
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

        Logger.LogWarning("Unknown weapon type: {0} for item {1}", weaponType, objectData.VNum);
        return WeaponTypes.Exotic;
    }

    private (SchoolTypes schoolType, IWeaponFlags weaponFlags, string damageNoun) ConvertWeaponDamageTypeFlagsAndNoun(ObjectData objectData)
    {
        string attackTable = (string)objectData.Values[3];
        SchoolTypes schoolType = SchoolTypes.None;
        string damageNoun = attackTable;
        (string name, string noun, int damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == attackTable);
        if (!attackTableEntry.Equals(default))
        {
            schoolType = ConvertDamageType(attackTableEntry.damType, $"item {objectData.VNum}");
            damageNoun = attackTableEntry.noun;
        }

        long weaponType2 = objectData.Values[4] == null ? 0L : System.Convert.ToInt64(objectData.Values[4]);
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

    private FurnitureActions ConvertFurnitureActions(ObjectData objectData)
    {
        FurnitureActions actions = FurnitureActions.None;
        int flag = objectData.Values[2] == null ? 0 : System.Convert.ToInt32(objectData.Values[2]);
        if (IsSet(flag, STAND_AT) || IsSet(flag, STAND_ON) || IsSet(flag, STAND_IN)) actions |= FurnitureActions.Stand;
        if (IsSet(flag, SIT_AT) || IsSet(flag, SIT_ON) || IsSet(flag, SIT_IN)) actions |= FurnitureActions.Sit;
        if (IsSet(flag, REST_AT) || IsSet(flag, REST_ON) || IsSet(flag, REST_IN) || IsSet(flag, SLEEP_ON)) actions |= FurnitureActions.Rest;
        if (IsSet(flag, SLEEP_AT) || IsSet(flag, SLEEP_ON) || IsSet(flag, SLEEP_IN) || IsSet(flag, SLEEP_ON)) actions |= FurnitureActions.Sleep;
        return actions;
    }

    private FurniturePlacePrepositions ConvertFurniturePreposition(ObjectData objectData)
    {
        int flag = objectData.Values[2] == null ? 0 : System.Convert.ToInt32(objectData.Values[2]);
        if (flag == 0)
            return FurniturePlacePrepositions.None;
        if (IsSet(flag, STAND_AT) || IsSet(flag, SIT_AT) || IsSet(flag, REST_AT) || IsSet(flag, SLEEP_AT)) return FurniturePlacePrepositions.At;
        if (IsSet(flag, STAND_ON) || IsSet(flag, SIT_ON) || IsSet(flag, REST_ON) || IsSet(flag, SLEEP_ON)) return FurniturePlacePrepositions.On;
        if (IsSet(flag, STAND_IN) || IsSet(flag, SIT_IN) || IsSet(flag, REST_IN) || IsSet(flag, SLEEP_IN)) return FurniturePlacePrepositions.In;
        Logger.LogWarning("Unknown Furniture preposition {0} for item {1}", flag, objectData.VNum);
        return FurniturePlacePrepositions.None;
    }

    private PortalFlags ConvertPortalFlags(ObjectData objectData)
    {
        // v1: exit flags
        // v2: gate flags
        PortalFlags flags = PortalFlags.None;
        long v1 = System.Convert.ToInt64(objectData.Values[1]);
        if (IsSet(v1, EX_CLOSED)) flags |= PortalFlags.Closed;
        if (IsSet(v1, EX_LOCKED)) flags |= PortalFlags.Locked;
        if (IsSet(v1, EX_PICKPROOF)) flags |= PortalFlags.PickProof;
        if (IsSet(v1, EX_EASY)) flags |= PortalFlags.Easy;
        if (IsSet(v1, EX_HARD)) flags |= PortalFlags.Hard;
        if (IsSet(v1, EX_NOCLOSE)) flags |= PortalFlags.NoClose;
        if (IsSet(v1, EX_NOLOCK)) flags |= PortalFlags.NoLock;
        long v2 = System.Convert.ToInt32(objectData.Values[2]);
        if (IsSet(v2, GATE_NOCURSE)) flags |= PortalFlags.NoCurse;
        if (IsSet(v2, GATE_GOWITH)) flags |= PortalFlags.GoWith;
        if (IsSet(v2, GATE_BUGGY)) flags |= PortalFlags.Buggy;
        if (IsSet(v2, GATE_RANDOM)) flags |= PortalFlags.Random;
        return flags;
    }

    private ContainerFlags ConvertContainerFlags(ObjectData objectData)
    {
        ContainerFlags flags = ContainerFlags.None;

        long v1 = System.Convert.ToInt64(objectData.Values[1]);
        if (!IsSet(v1, CONT_CLOSEABLE)) flags |= ContainerFlags.NoClose;
        if (IsSet(v1, CONT_PICKPROOF)) flags |= ContainerFlags.PickProof;
        if (IsSet(v1, CONT_CLOSED)) flags |= ContainerFlags.Closed;
        if (IsSet(v1, CONT_LOCKED)) flags |= ContainerFlags.Locked;

        long v2 = System.Convert.ToInt64(objectData.Values[2]);
        if (v2 <= 0) flags |= ContainerFlags.NoLock;

        return flags;
    }

    // wear flags
    private const long ITEM_TAKE = RotLoader.A;
    private const long ITEM_WEAR_FINGER = RotLoader.B;
    private const long ITEM_WEAR_NECK = RotLoader.C;
    private const long ITEM_WEAR_BODY = RotLoader.D;
    private const long ITEM_WEAR_HEAD = RotLoader.E;
    private const long ITEM_WEAR_LEGS = RotLoader.F;
    private const long ITEM_WEAR_FEET = RotLoader.G;
    private const long ITEM_WEAR_HANDS = RotLoader.H;
    private const long ITEM_WEAR_ARMS = RotLoader.I;
    private const long ITEM_WEAR_SHIELD = RotLoader.J;
    private const long ITEM_WEAR_ABOUT = RotLoader.K;
    private const long ITEM_WEAR_WAIST = RotLoader.L;
    private const long ITEM_WEAR_WRIST = RotLoader.M;
    private const long ITEM_WIELD = RotLoader.N;
    private const long ITEM_HOLD = RotLoader.O;
    private const long ITEM_NO_SAC = RotLoader.P;
    private const long ITEM_WEAR_FLOAT = RotLoader.Q;
    private const long ITEM_WEAR_FACE = RotLoader.R;

    // extra flags
    private const long ITEM_GLOW = RotLoader.A;
    private const long ITEM_HUM = RotLoader.B;
    private const long ITEM_DARK = RotLoader.C;
    private const long ITEM_LOCK = RotLoader.D;
    private const long ITEM_EVIL = RotLoader.E;
    private const long ITEM_INVIS = RotLoader.F;
    private const long ITEM_MAGIC = RotLoader.G;
    private const long ITEM_NODROP = RotLoader.H;
    private const long ITEM_BLESS = RotLoader.I;
    private const long ITEM_ANTI_GOOD = RotLoader.J;
    private const long ITEM_ANTI_EVIL = RotLoader.K;
    private const long ITEM_ANTI_NEUTRAL = RotLoader.L;
    private const long ITEM_NOREMOVE = RotLoader.M;
    private const long ITEM_INVENTORY = RotLoader.N;
    private const long ITEM_NOPURGE = RotLoader.O;
    private const long ITEM_ROT_DEATH = RotLoader.P;
    private const long ITEM_VIS_DEATH = RotLoader.Q;
    private const long ITEM_NOSAC = RotLoader.R;
    private const long ITEM_NONMETAL = RotLoader.S;
    private const long ITEM_NOLOCATE = RotLoader.T;
    private const long ITEM_MELT_DROP = RotLoader.U;
    private const long ITEM_HAD_TIMER = RotLoader.V;
    private const long ITEM_SELL_EXTRACT = RotLoader.W;
    private const long ITEM_BURN_PROOF = RotLoader.Y;
    private const long ITEM_NOUNCURSE = RotLoader.Z;
    private const long ITEM_LQUEST = RotLoader.aa;
    private const long ITEM_FORCED = RotLoader.bb;
    private const long ITEM_QUESTPOINT = RotLoader.cc;
    private const long ITEM_QUEST = RotLoader.dd;

    // weapon types
    private const long WEAPON_FLAMING = RotLoader.A;
    private const long WEAPON_FROST = RotLoader.B;
    private const long WEAPON_VAMPIRIC = RotLoader.C;
    private const long WEAPON_SHARP = RotLoader.D;
    private const long WEAPON_VORPAL = RotLoader.E;
    private const long WEAPON_TWO_HANDS = RotLoader.F;
    private const long WEAPON_SHOCKING = RotLoader.G;
    private const long WEAPON_POISON = RotLoader.H;

    // furniture flags
    private const long STAND_AT = RotLoader.A;
    private const long STAND_ON = RotLoader.B;
    private const long STAND_IN = RotLoader.C;
    private const long SIT_AT = RotLoader.D;
    private const long SIT_ON = RotLoader.E;
    private const long SIT_IN = RotLoader.F;
    private const long REST_AT = RotLoader.G;
    private const long REST_ON = RotLoader.H;
    private const long REST_IN = RotLoader.I;
    private const long SLEEP_AT = RotLoader.J;
    private const long SLEEP_ON = RotLoader.K;
    private const long SLEEP_IN = RotLoader.L;
    private const long PUT_AT = RotLoader.M;
    private const long PUT_ON = RotLoader.N;
    private const long PUT_IN = RotLoader.O;
    private const long PUT_INSIDE = RotLoader.P;

    // container flags
    private const long CONT_CLOSEABLE = 1;
    private const long CONT_PICKPROOF = 2;
    private const long CONT_CLOSED = 4;
    private const long CONT_LOCKED = 8;
    private const long CONT_PUT_ON = 16;

    // portal flags
    private const long GATE_NORMAL_EXIT = RotLoader.A;
    private const long GATE_NOCURSE = RotLoader.B;
    private const long GATE_GOWITH = RotLoader.C;
    private const long GATE_BUGGY = RotLoader.D;
    private const long GATE_RANDOM = RotLoader.E;

    #endregion

    #region Mobile

    private CharacterBlueprintBase ConvertMobile(MobileData mobileData)
    {
        if (_characterBlueprints.Any(x => x.Id == mobileData.VNum))
            RaiseConvertException("Duplicate mobile Id {0}", mobileData.VNum);

        SchoolTypes schoolType = SchoolTypes.None;
        string damageNoun = mobileData.DamType;
        (string name, string noun, int damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == mobileData.DamType);
        if (!attackTableEntry.Equals(default))
        {
            schoolType = ConvertDamageType(attackTableEntry.damType, $"mob {mobileData.VNum}");
            damageNoun = attackTableEntry.noun;
        }

        (IOffensiveFlags offensiveFlags, IAssistFlags assistFlags) = ConvertOffensiveFlags(mobileData.OffFlags);
        (ICharacterFlags characterFlags, IShieldFlags shieldFlags) = ConvertCharacterFlagsAndShieldFlags(mobileData.AffectedBy, mobileData.ShieldedBy);

        if (mobileData.Shop == null)
        {
            // ShieldedBy, Group, StartPos, DefaultPos, Form, Parts, Material not used
            return new CharacterNormalBlueprint
            {
                Id = mobileData.VNum,
                Name = mobileData.PlayerName,
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
                DamageDiceCount = mobileData.Dam[0],
                DamageDiceValue = mobileData.Dam[1],
                DamageDiceBonus = mobileData.Dam[2],
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
                ActFlags = ConvertActFlags(mobileData.Act, mobileData.Act2),
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
        }
        else 
        {
            return new CharacterShopBlueprint
            {
                Id = mobileData.VNum,
                Name = mobileData.PlayerName,
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
                DamageDiceCount = mobileData.Dam[0],
                DamageDiceValue = mobileData.Dam[1],
                DamageDiceBonus = mobileData.Dam[2],
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
                ActFlags = ConvertActFlags(mobileData.Act, mobileData.Act2),
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

    private (ICharacterFlags characterFlags, IShieldFlags shieldFlags) ConvertCharacterFlagsAndShieldFlags(long affectedBy, long shieldedBy)
    {
        var characterFlags = new CharacterFlags();
        var shieldFlags = new ShieldFlags();
        if (IsSet(affectedBy, AFF_BLIND)) characterFlags.Set("Blind");
        if (IsSet(affectedBy, AFF_DETECT_EVIL)) characterFlags.Set("DetectEvil");
        if (IsSet(affectedBy, AFF_DETECT_INVIS)) characterFlags.Set("DetectInvis");
        if (IsSet(affectedBy, AFF_DETECT_MAGIC)) characterFlags.Set("DetectMagic");
        if (IsSet(affectedBy, AFF_DETECT_HIDDEN)) characterFlags.Set("DetectHidden");
        if (IsSet(affectedBy, AFF_DETECT_GOOD)) characterFlags.Set("DetectGood");
        if (IsSet(affectedBy, AFF_FAERIE_FIRE)) characterFlags.Set("FaerieFire");
        if (IsSet(affectedBy, AFF_INFRARED)) characterFlags.Set("Infrared");
        if (IsSet(affectedBy, AFF_CURSE)) characterFlags.Set("Curse");
        // AFF_FARSIGHT
        if (IsSet(affectedBy, AFF_POISON)) characterFlags.Set("Poison");
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

        // SHD_PROTECT_VOODOO
        if (IsSet(shieldedBy, SHD_INVISIBLE)) characterFlags.Set("Invisible");
        if (IsSet(shieldedBy, SHD_ICE)) shieldFlags.Set("IceShield");
        if (IsSet(shieldedBy, SHD_FIRE)) shieldFlags.Set("FireShield");
        if (IsSet(shieldedBy, SHD_SHOCK)) shieldFlags.Set("ShockShield");
        if (IsSet(shieldedBy, SHD_SANCTUARY)) shieldFlags.Set("Sanctuary");
        if (IsSet(shieldedBy, SHD_PROTECT_EVIL)) shieldFlags.Set("ProtectEvil");
        if (IsSet(shieldedBy, SHD_PROTECT_GOOD)) shieldFlags.Set("ProtectGood");


        return (characterFlags, shieldFlags);
    }

    private IActFlags ConvertActFlags(long act, long act2)
    {
        var flags = new ActFlags();
        //ACT_IS_NPC not used
        if (IsSet(act, ACT_SENTINEL)) flags.Set("Sentinel");
        if (IsSet(act, ACT_SCAVENGER)) flags.Set("Scavenger");
        if (IsSet(act, ACT_AGGRESSIVE)) flags.Set("Aggressive");
        // ACT_KEY
        // ACT_RANGER
        if (IsSet(act, ACT_STAY_AREA)) flags.Set("StayArea");
        if (IsSet(act, ACT_WIMPY)) flags.Set("Wimpy");
        if (IsSet(act, ACT_PET)) flags.Set("Pet");
        if (IsSet(act, ACT_TRAIN)) flags.Set("Train");
        if (IsSet(act, ACT_PRACTICE)) flags.Set("Practice");
        // ACT_DRUID
        // ACT_NO_BODY
        // ACT_NB_DROP
        if (IsSet(act, ACT_UNDEAD)) flags.Set("Undead");
        // ACT_VAMPIRE
        if (IsSet(act, ACT_CLERIC)) flags.Set("Cleric");
        if (IsSet(act, ACT_MAGE)) flags.Set("Mage");
        if (IsSet(act, ACT_THIEF)) flags.Set("Thief");
        if (IsSet(act, ACT_WARRIOR)) flags.Set("Warrior");
        if (IsSet(act, ACT_NOALIGN)) flags.Set("NoAlign");
        if (IsSet(act, ACT_NOPURGE)) flags.Set("NoPurge");
        if (IsSet(act, ACT_OUTDOORS)) flags.Set("Outdoors");
        // ACT_IS_SATAN
        if (IsSet(act, ACT_INDOORS)) flags.Set("Indoors");
        if (IsSet(act, ACT_IS_HEALER)) flags.Set("IsHealer");
        // ACT_IS_PRIEST
        if (IsSet(act, ACT_GAIN)) flags.Set("Gain");
        if (IsSet(act, ACT_UPDATE_ALWAYS)) flags.Set("UpdateAlways");
        // ACT_IS_BANKER
        // ACT_QUESTMASTER
        // ACT2_FORGER
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
        // OFF_FEED
        // OFF_CLAN_GUARD

        var assist = new AssistFlags();
        if (IsSet(input, ASSIST_ALL)) assist.Set("All");
        if (IsSet(input, ASSIST_ALIGN)) assist.Set("Align");
        if (IsSet(input, ASSIST_RACE)) assist.Set("Race");
        if (IsSet(input, ASSIST_PLAYERS)) assist.Set("Players");
        if (IsSet(input, ASSIST_GUARD)) assist.Set("Guard");
        if (IsSet(input, ASSIST_VNUM)) assist.Set("Vnum");

        return (off, assist);
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

    private IEnumerable<Type> ConvertBuyTypes(ShopData shopData)
    {
        foreach (int buyType in shopData.BuyType)
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
                    Logger.LogWarning("Invalid buy type {0} for mob {1}", buyType, shopData.Keeper);
                    break;
                case ITEM_MAP: yield return typeof(ItemMapBlueprint); break;
                case ITEM_PORTAL: yield return typeof(ItemPortalBlueprint); break;
                case ITEM_WARP_STONE: yield return typeof(ItemWarpStoneBlueprint); break;
                case ITEM_ROOM_KEY:
                    Logger.LogWarning("Invalid buy type {0} for mob {1}", buyType, shopData.Keeper);
                    break;
                case ITEM_GEM: yield return typeof(ItemGemBlueprint); break;
                case ITEM_JEWELRY: yield return typeof(ItemJewelryBlueprint); break;
                case ITEM_JUKEBOX: yield return typeof(ItemJukeboxBlueprint); break;
                case ITEM_DEMON_STONE:
                case ITEM_EXIT:
                case ITEM_PIT:
                case ITEM_PASSBOOK:
                case ITEM_VEHICLE:
                default:
                    Logger.LogWarning("Invalid buy type {0} for mob {1}", buyType, shopData.Keeper);
                    break;
            }
        }
    }

    // Immunites, Resistances, Vulnerabilities
    private const long IMM_SUMMON = RotLoader.A;
    private const long IMM_CHARM = RotLoader.B;
    private const long IMM_MAGIC = RotLoader.C;
    private const long IMM_WEAPON = RotLoader.D;
    private const long IMM_BASH = RotLoader.E;
    private const long IMM_PIERCE = RotLoader.F;
    private const long IMM_SLASH = RotLoader.G;
    private const long IMM_FIRE = RotLoader.H;
    private const long IMM_COLD = RotLoader.I;
    private const long IMM_LIGHTNING = RotLoader.J;
    private const long IMM_ACID = RotLoader.K;
    private const long IMM_POISON = RotLoader.L;
    private const long IMM_NEGATIVE = RotLoader.M;
    private const long IMM_HOLY = RotLoader.N;
    private const long IMM_ENERGY = RotLoader.O;
    private const long IMM_MENTAL = RotLoader.P;
    private const long IMM_DISEASE = RotLoader.Q;
    private const long IMM_DROWNING = RotLoader.R;
    private const long IMM_LIGHT = RotLoader.S;
    private const long IMM_SOUND = RotLoader.T;
    private const long IMM_WOOD = RotLoader.X;
    private const long IMM_SILVER = RotLoader.Y;
    private const long IMM_IRON = RotLoader.Z;

    // Affected by
    private const long AFF_BLIND = RotLoader.A;
    private const long AFF_DETECT_EVIL = RotLoader.C;
    private const long AFF_DETECT_INVIS = RotLoader.D;
    private const long AFF_DETECT_MAGIC = RotLoader.E;
    private const long AFF_DETECT_HIDDEN = RotLoader.F;
    private const long AFF_DETECT_GOOD = RotLoader.G;
    private const long AFF_FAERIE_FIRE = RotLoader.I;
    private const long AFF_INFRARED = RotLoader.J;
    private const long AFF_CURSE = RotLoader.K;
    private const long AFF_FARSIGHT = RotLoader.L;
    private const long AFF_POISON = RotLoader.M;
    private const long AFF_SNEAK = RotLoader.P;
    private const long AFF_HIDE = RotLoader.Q;
    private const long AFF_SLEEP = RotLoader.R;
    private const long AFF_CHARM = RotLoader.S;
    private const long AFF_FLYING = RotLoader.T;
    private const long AFF_PASS_DOOR = RotLoader.U;
    private const long AFF_HASTE = RotLoader.V;
    private const long AFF_CALM = RotLoader.W;
    private const long AFF_PLAGUE = RotLoader.X;
    private const long AFF_WEAKEN = RotLoader.Y;
    private const long AFF_DARK_VISION = RotLoader.Z;
    private const long AFF_BERSERK = RotLoader.aa;
    private const long AFF_SWIM = RotLoader.bb;
    private const long AFF_REGENERATION = RotLoader.cc;
    private const long AFF_SLOW = RotLoader.dd;

    // Shield
    private const long SHD_PROTECT_VOODOO = RotLoader.A;
    private const long SHD_INVISIBLE = RotLoader.B;
    private const long SHD_ICE = RotLoader.C;
    private const long SHD_FIRE = RotLoader.D;
    private const long SHD_SHOCK = RotLoader.E;
    private const long SHD_SANCTUARY = RotLoader.H;
    private const long SHD_PROTECT_EVIL = RotLoader.N;
    private const long SHD_PROTECT_GOOD = RotLoader.O;

    // Act flags
    private const long ACT_IS_NPC = RotLoader.A;
    private const long ACT_SENTINEL = RotLoader.B;
    private const long ACT_SCAVENGER = RotLoader.C;
    private const long ACT_KEY = RotLoader.D;
    private const long ACT_RANGER = RotLoader.E;
    private const long ACT_AGGRESSIVE = RotLoader.F;
    private const long ACT_STAY_AREA = RotLoader.G;
    private const long ACT_WIMPY = RotLoader.H;
    private const long ACT_PET = RotLoader.I;
    private const long ACT_TRAIN = RotLoader.J;
    private const long ACT_PRACTICE = RotLoader.K;
    private const long ACT_DRUID = RotLoader.L;
    private const long ACT_NO_BODY = RotLoader.M; // no corpse
    private const long ACT_NB_DROP = RotLoader.N; // corpseless, drop all
    private const long ACT_UNDEAD = RotLoader.O;
    private const long ACT_VAMPIRE = RotLoader.P;
    private const long ACT_CLERIC = RotLoader.Q;
    private const long ACT_MAGE = RotLoader.R;
    private const long ACT_THIEF = RotLoader.S;
    private const long ACT_WARRIOR = RotLoader.T;
    private const long ACT_NOALIGN = RotLoader.U;
    private const long ACT_NOPURGE = RotLoader.V;
    private const long ACT_OUTDOORS = RotLoader.W;
    private const long ACT_IS_SATAN = RotLoader.X;
    private const long ACT_INDOORS = RotLoader.Y;
    private const long ACT_IS_PRIEST = RotLoader.Z;
    private const long ACT_IS_HEALER = RotLoader.aa;
    private const long ACT_GAIN = RotLoader.bb;
    private const long ACT_UPDATE_ALWAYS = RotLoader.cc;
    private const long ACT_IS_BANKER = RotLoader.dd;
    private const long ACT_QUESTMASTER = RotLoader.ee;

    // Act flags2
    private const long ACT2_IS_NPC = RotLoader.A;
    private const long ACT2_FORGER = RotLoader.B;

    // Offensive flags
    private const long OFF_AREA_ATTACK = RotLoader.A;
    private const long OFF_BACKSTAB = RotLoader.B;
    private const long OFF_BASH = RotLoader.C;
    private const long OFF_BERSERK = RotLoader.D;
    private const long OFF_DISARM = RotLoader.E;
    private const long OFF_DODGE = RotLoader.F;
    private const long OFF_FADE = RotLoader.G;
    private const long OFF_FAST = RotLoader.H;
    private const long OFF_KICK = RotLoader.I;
    private const long OFF_KICK_DIRT = RotLoader.J;
    private const long OFF_PARRY = RotLoader.K;
    private const long OFF_RESCUE = RotLoader.L;
    private const long OFF_TAIL = RotLoader.M;
    private const long OFF_TRIP = RotLoader.N;
    private const long OFF_CRUSH = RotLoader.O;
    private const long ASSIST_ALL = RotLoader.P;
    private const long ASSIST_ALIGN = RotLoader.Q;
    private const long ASSIST_RACE = RotLoader.R;
    private const long ASSIST_PLAYERS = RotLoader.S;
    private const long ASSIST_GUARD = RotLoader.T;
    private const long ASSIST_VNUM = RotLoader.U;
    private const long OFF_FEED = RotLoader.V;
    private const long OFF_CLAN_GUARD = RotLoader.W;

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
    private const int ITEM_DEMON_STONE = 35;
    private const int ITEM_EXIT = 36;
    private const int ITEM_PIT = 37;
    private const int ITEM_PASSBOOK = 38;
    private const int ITEM_VEHICLE = 39;

    // body form
    private const long FORM_EDIBLE = RotLoader.A;
    private const long FORM_POISON = RotLoader.B;
    private const long FORM_MAGICAL = RotLoader.C;
    private const long FORM_INSTANT_DECAY = RotLoader.D;
    private const long FORM_OTHER = RotLoader.E;  /* defined by material bit */
    // actual form
    private const long FORM_ANIMAL = RotLoader.G;
    private const long FORM_SENTIENT = RotLoader.H;
    private const long FORM_UNDEAD = RotLoader.I;
    private const long FORM_CONSTRUCT = RotLoader.J;
    private const long FORM_MIST = RotLoader.K;
    private const long FORM_INTANGIBLE = RotLoader.L;
    private const long FORM_BIPED = RotLoader.M;
    private const long FORM_CENTAUR = RotLoader.N;
    private const long FORM_INSECT = RotLoader.O;
    private const long FORM_SPIDER = RotLoader.P;
    private const long FORM_CRUSTACEAN = RotLoader.Q;
    private const long FORM_WORM = RotLoader.R;
    private const long FORM_BLOB = RotLoader.S;
    private const long FORM_MAMMAL = RotLoader.V;
    private const long FORM_BIRD = RotLoader.W;
    private const long FORM_REPTILE = RotLoader.X;
    private const long FORM_SNAKE = RotLoader.Y;
    private const long FORM_DRAGON = RotLoader.Z;
    private const long FORM_AMPHIBIAN = RotLoader.aa;
    private const long FORM_FISH = RotLoader.bb;
    private const long FORM_COLD_BLOOD = RotLoader.cc;

    // body parts
    private const long PART_HEAD = RotLoader.A;
    private const long PART_ARMS = RotLoader.B;
    private const long PART_LEGS = RotLoader.C;
    private const long PART_HEART = RotLoader.D;
    private const long PART_BRAINS = RotLoader.E;
    private const long PART_GUTS = RotLoader.F;
    private const long PART_HANDS = RotLoader.G;
    private const long PART_FEET = RotLoader.H;
    private const long PART_FINGERS = RotLoader.I;
    private const long PART_EAR = RotLoader.J;
    private const long PART_EYE = RotLoader.K;
    private const long PART_LONG_TONGUE = RotLoader.L;
    private const long PART_EYESTALKS = RotLoader.M;
    private const long PART_TENTACLES = RotLoader.N;
    private const long PART_FINS = RotLoader.O;
    private const long PART_WINGS = RotLoader.P;
    private const long PART_TAIL = RotLoader.Q;
    // for combat
    private const long PART_CLAWS = RotLoader.U;
    private const long PART_FANGS = RotLoader.V;
    private const long PART_HORNS = RotLoader.W;
    private const long PART_SCALES = RotLoader.X;
    private const long PART_TUSKS = RotLoader.Y;

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

        Logger.LogWarning("Unknown damage type {0} for {1}", damageType, errorMsg);
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
        ("typo", "typo", DAM_SLASH ), /* 40 */
        ("tail", "thrash", DAM_SLASH),
    };

    //
    private bool IsSet(long input, long bit) => (input & bit) == bit;

    private int ConvertObjectToInt32(object value)
    {
        if (value is long value64Bits)
        {
            long value32Bits;
            if (value64Bits > int.MaxValue)
                value32Bits = value64Bits + 2 * ((long)int.MinValue);
            else
                value32Bits = (int)value64Bits;
            return (int)value32Bits;
        }
        else if (value is int value32Bits)
            return value32Bits;
        RaiseConvertException("Unable to convert object value to int32");
        return 0;
    }

    //
    private void RaiseConvertException(string format, params object[] parameters)
    {
        string message = string.Format(format, parameters);
        Logger.LogError(message);
        throw new RotConvertException(message);
    }

    //
    private string RemoveCommentIfAny(string filename)
    {
        int index = filename.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
        if (index >= 0)
            return filename.Remove(index).Trim();
        return filename;
    }
}
