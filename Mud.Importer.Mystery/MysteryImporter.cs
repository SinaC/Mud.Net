using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Mud.Importer.Mystery
{
    public class MysteryImporter
    {
        private readonly List<AreaBlueprint> _areaBlueprints = new List<AreaBlueprint>();
        private readonly List<RoomBlueprint> _roomBlueprints = new List<RoomBlueprint>();
        private readonly List<ItemBlueprintBase> _itemBlueprints = new List<ItemBlueprintBase>();
        private readonly List<CharacterBlueprintBase> _characterBlueprints = new List<CharacterBlueprintBase>();

        public IReadOnlyCollection<AreaBlueprint> Areas => _areaBlueprints.AsReadOnly();
        public IReadOnlyCollection<RoomBlueprint> Rooms => _roomBlueprints.AsReadOnly();
        public IReadOnlyCollection<ItemBlueprintBase> Items => _itemBlueprints.AsReadOnly();
        public IReadOnlyCollection<CharacterBlueprintBase> Characters => _characterBlueprints.AsReadOnly();

        public void ImportByList(string path, string areaLst)
        {
            MysteryLoader loader = new MysteryLoader();
            string[] areaFilenames = File.ReadAllLines(Path.Combine(path, areaLst));
            foreach (string areaFilename in areaFilenames)
            {
                if (areaFilename.Contains("$"))
                    break;
                if (areaFilename.StartsWith("-"))
                    Log.Default.WriteLine(LogLevels.Info, "Skipping {0}", areaFilename);
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
            MysteryLoader loader = new MysteryLoader();
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
            MysteryLoader loader = new MysteryLoader();
            foreach (string filename in filenames)
            {
                string fullName = Path.Combine(path, filename);
                loader.Load(fullName);
                loader.Parse();
            }

            Convert(loader);
        }

        private void Convert(MysteryLoader loader)
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
                Flags = ConvertAreaFlags(areaData.Flags),
                Security = areaData.Security
            };
        }

        private AreaFlags ConvertAreaFlags(long input)
        {
            AreaFlags flags = AreaFlags.None;
            if (IsSet(input, AREA_CHANGED)) flags |= AreaFlags.Changed;
            if (IsSet(input, AREA_ADDED)) flags |= AreaFlags.Added;
            if (IsSet(input, AREA_LOADING)) flags |= AreaFlags.Loading;
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
            ExitBlueprint[] blueprints = new ExitBlueprint[RoomData.MaxExits];
            for (int i = 0; i < RoomData.MaxExits; i++)
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
                    blueprints[i] = null;
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

        private RoomFlags ConvertRoomFlags(long input)
        {
            RoomFlags flags = RoomFlags.None;
            if (IsSet(input, ROOM_DARK)) flags |= RoomFlags.Dark;
            if (IsSet(input, ROOM_NO_MOB)) flags |= RoomFlags.NoMob;
            if (IsSet(input, ROOM_INDOORS)) flags |= RoomFlags.Indoors;
            if (IsSet(input, ROOM_PRIVATE)) flags |= RoomFlags.Private;
            if (IsSet(input, ROOM_SAFE)) flags |= RoomFlags.Safe;
            if (IsSet(input, ROOM_SOLITARY)) flags |= RoomFlags.Solitary;
            //TODO: ROOM_PET_SHOP
            if (IsSet(input, ROOM_NO_RECALL)) flags |= RoomFlags.NoRecall;
            if (IsSet(input, ROOM_IMP_ONLY)) flags |= RoomFlags.ImpOnly;
            if (IsSet(input, ROOM_GODS_ONLY)) flags |= RoomFlags.GodsOnly;
            //TODO: ROOM_HEROES_ONLY
            if (IsSet(input, ROOM_NEWBIES_ONLY)) flags |= RoomFlags.NewbiesOnly;
            if (IsSet(input, ROOM_LAW)) flags |= RoomFlags.Law;
            if (IsSet(input, ROOM_NOWHERE)) flags |= RoomFlags.NoWhere;

            return flags;
        }

        private ExitFlags ConvertExitFlags(long input)
        {
            ExitFlags flags = 0;
            if (IsSet(input, EX_ISDOOR)) flags |= ExitFlags.Door;
            if (IsSet(input, EX_CLOSED)) flags |= ExitFlags.Closed;
            if (IsSet(input, EX_LOCKED)) flags |= ExitFlags.Locked;
            if (IsSet(input, EX_PICKPROOF)) flags |= ExitFlags.PickProof;
            if (IsSet(input, EX_NOPASS)) flags |= ExitFlags.NoPass;
            if (IsSet(input, EX_EASY)) flags |= ExitFlags.Easy;
            if (IsSet(input, EX_HARD)) flags |= ExitFlags.Hard;
            // EX_INFURIATING
            // EX_NOCLOSE
            // EX_NOLOCK
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
        private const long ROOM_DARK = MysteryLoader.A;
        private const long ROOM_NO_MOB = MysteryLoader.C;
        private const long ROOM_INDOORS = MysteryLoader.D;
        private const long ROOM_PRIVATE = MysteryLoader.J;
        private const long ROOM_SAFE = MysteryLoader.K;
        private const long ROOM_SOLITARY = MysteryLoader.L;
        private const long ROOM_PET_SHOP = MysteryLoader.M;
        private const long ROOM_NO_RECALL = MysteryLoader.N;
        private const long ROOM_IMP_ONLY = MysteryLoader.O;
        private const long ROOM_GODS_ONLY = MysteryLoader.P;
        private const long ROOM_HEROES_ONLY = MysteryLoader.Q;
        private const long ROOM_NEWBIES_ONLY = MysteryLoader.R;
        private const long ROOM_LAW = MysteryLoader.S;
        private const long ROOM_NOWHERE = MysteryLoader.T;

        // Exit flags
        private const long EX_ISDOOR = MysteryLoader.A;
        private const long EX_CLOSED = MysteryLoader.B;
        private const long EX_LOCKED = MysteryLoader.C;
        private const long EX_PICKPROOF = MysteryLoader.F;
        private const long EX_NOPASS = MysteryLoader.G;
        private const long EX_EASY = MysteryLoader.H;
        private const long EX_HARD = MysteryLoader.I;
        private const long EX_INFURIATING = MysteryLoader.J;
        private const long EX_NOCLOSE = MysteryLoader.K;
        private const long EX_NOLOCK = MysteryLoader.L;

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
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset M arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
                        if (reset.Arg4 == 0)
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset M arg4 (local limit) is 0 for room id '{roomData.VNum}'.");
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
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset O arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
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
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset P arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
                        if (reset.Arg4 == 0)
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset P arg4 (local limit) is 0 for room id '{roomData.VNum}'.");
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
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset G arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
                        yield return new ItemInCharacterReset
                        {
                            RoomId = roomData.VNum,
                            ItemId = reset.Arg1,
                            GlobalLimit = reset.Arg2,
                        };
                        break;
                    case 'E':
                        if (reset.Arg2 == 0)
                            Log.Default.WriteLine(LogLevels.Warning, $"Reset E arg2 (global limit) is 0 for room id '{roomData.VNum}'.");
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
                            Operation = reset.Arg3
                        };
                        break;
                    case 'R':
                        yield return new RandomizeExitsReset
                        {
                            RoomId = roomData.VNum,
                            MaxDoors = reset.Arg2
                        };
                        break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Unknown Reset {0} for room {1}", reset.Command, roomData.VNum);
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
                // WEAR_EAR_L = 18;
                // WEAR_EAR_R = 19;
                // WEAR_EYES = 20;
                case WEAR_SECONDARY: return EquipmentSlots.OffHand;
                case WEAR_FLOAT: return EquipmentSlots.Float;
                case WEAR_THIRDLY: return EquipmentSlots.MainHand;
                case WEAR_FOURTHLY: return EquipmentSlots.OffHand;
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
        private const int WEAR_EAR_L = 18;
        private const int WEAR_EAR_R = 19;
        private const int WEAR_EYES = 20;
        private const int WEAR_SECONDARY = 21;
        private const int WEAR_FLOAT = 22; // was 18
        private const int WEAR_THIRDLY = 23;
        private const int WEAR_FOURTHLY = 24;

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
                    (SchoolTypes damageType, WeaponFlags weaponFlags, string damageNoun) weaponInfo = ConvertWeaponDamageTypeFlagsAndNoun(objectData);
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
                //TODO: case "protect":
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
                //TODO: case "component": replace room_key
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
                //TODO: instrument
                //TODO: window
                //TODO: template
                //TODO: saddle
                //TODO: rope

                default:
                    Log.Default.WriteLine(LogLevels.Warning, $"ItemBlueprint cannot be created: Vnum: [{objectData.VNum}] Type: {objectData.ItemType} Flags: {objectData.WearFlags} : {objectData.Name}");
                    break;
            }

            return null;
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
                //TODO: ITEM_WEAR_EAR
                //TODO: ITEM_WEAR_EYES
            }

            Log.Default.WriteLine(LogLevels.Warning, "Unknown wear location: {0} for item {1}", objectData.WearFlags, objectData.VNum);
            return WearLocations.None;
        }

        private ItemFlags ConvertExtraFlags(ObjectData objectData)
        {
            ItemFlags itemFlags = ItemFlags.None;

            if (IsSet(objectData.ExtraFlags, ITEM_GLOW)) itemFlags |= ItemFlags.Glowing;
            if (IsSet(objectData.ExtraFlags, ITEM_HUM)) itemFlags |= ItemFlags.Humming;
            if (IsSet(objectData.ExtraFlags, ITEM_DARK)) itemFlags |= ItemFlags.Dark;
            //STAY_DEATH if (IsSet(objectData.ExtraFlags, ITEM_LOCK)) itemFlags |= ItemFlags.Lock;
            if (IsSet(objectData.ExtraFlags, ITEM_EVIL)) itemFlags |= ItemFlags.Evil;
            if (IsSet(objectData.ExtraFlags, ITEM_INVIS)) itemFlags |= ItemFlags.Invis;
            if (IsSet(objectData.ExtraFlags, ITEM_MAGIC)) itemFlags |= ItemFlags.Magic;
            if (IsSet(objectData.ExtraFlags, ITEM_NODROP)) itemFlags |= ItemFlags.NoDrop;
            if (IsSet(objectData.ExtraFlags, ITEM_BLESS)) itemFlags |= ItemFlags.Bless;
            if (IsSet(objectData.ExtraFlags, ITEM_ANTI_GOOD)) itemFlags |= ItemFlags.AntiGood;
            if (IsSet(objectData.ExtraFlags, ITEM_ANTI_EVIL)) itemFlags |= ItemFlags.AntiEvil;
            if (IsSet(objectData.ExtraFlags, ITEM_ANTI_NEUTRAL)) itemFlags |= ItemFlags.AntiNeutral;
            if (IsSet(objectData.ExtraFlags, ITEM_NOREMOVE)) itemFlags |= ItemFlags.NoRemove;
            if (IsSet(objectData.ExtraFlags, ITEM_INVENTORY)) itemFlags |= ItemFlags.Inventory;
            if (IsSet(objectData.ExtraFlags, ITEM_NOPURGE)) itemFlags |= ItemFlags.NoPurge;
            if (IsSet(objectData.ExtraFlags, ITEM_ROT_DEATH)) itemFlags |= ItemFlags.RotDeath;
            if (IsSet(objectData.ExtraFlags, ITEM_VIS_DEATH)) itemFlags |= ItemFlags.VisibleDeath;
            //UNIQUE nonmetal replaced with material if (IsSet(objectData.ExtraFlags, ITEM_NONMETAL)) itemFlags |= ItemFlags.NonMetal;
            if (IsSet(objectData.ExtraFlags, ITEM_NOLOCATE)) itemFlags |= ItemFlags.NoLocate;
            if (IsSet(objectData.ExtraFlags, ITEM_MELT_DROP)) itemFlags |= ItemFlags.MeltOnDrop;
            if (IsSet(objectData.ExtraFlags, ITEM_HAD_TIMER)) itemFlags |= ItemFlags.HadTimer;
            if (IsSet(objectData.ExtraFlags, ITEM_SELL_EXTRACT)) itemFlags |= ItemFlags.SellExtract;
            if (IsSet(objectData.ExtraFlags, ITEM_BURN_PROOF)) itemFlags |= ItemFlags.BurnProof;
            if (IsSet(objectData.ExtraFlags, ITEM_NOUNCURSE)) itemFlags |= ItemFlags.NoUncurse;
            if (IsSet(objectData.ExtraFlags, ITEM_NOSAC)) itemFlags |= ItemFlags.NoSacrifice;
            // ITEM_NOIDENT
            // ITEM_NOCOND
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
                //TODO: staff(weapon)
                //TODO: arrow
                //TODO: ranged
            }

            Log.Default.WriteLine(LogLevels.Warning, "Unknown weapon type: {0} for item {1}", weaponType, objectData.VNum);
            return WeaponTypes.Exotic;
        }

        private (SchoolTypes schoolType, WeaponFlags weaponFlags, string damageNoun) ConvertWeaponDamageTypeFlagsAndNoun(ObjectData objectData)
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
            WeaponFlags weaponFlags = WeaponFlags.None;
            if (IsSet(weaponType2, WEAPON_FLAMING)) weaponFlags |= WeaponFlags.Flaming;
            if (IsSet(weaponType2, WEAPON_FROST)) weaponFlags |= WeaponFlags.Frost;
            if (IsSet(weaponType2, WEAPON_VAMPIRIC)) weaponFlags |= WeaponFlags.Vampiric;
            if (IsSet(weaponType2, WEAPON_SHARP)) weaponFlags |= WeaponFlags.Sharp;
            if (IsSet(weaponType2, WEAPON_VORPAL)) weaponFlags |= WeaponFlags.Vorpal;
            if (IsSet(weaponType2, WEAPON_TWO_HANDS)) weaponFlags |= WeaponFlags.TwoHands;
            if (IsSet(weaponType2, WEAPON_SHOCKING)) weaponFlags |= WeaponFlags.Shocking;
            if (IsSet(weaponType2, WEAPON_POISON)) weaponFlags |= WeaponFlags.Poison;

            //
            return (schoolType, weaponFlags, damageNoun);
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
            Log.Default.WriteLine(LogLevels.Warning, "Unknown Furniture preposition {0} for item {1}", flag, objectData.VNum);
            return FurniturePlacePrepositions.None;
        }

        // Wear flags
        private const long ITEM_TAKE = MysteryLoader.A;
        private const long ITEM_WEAR_FINGER = MysteryLoader.B;
        private const long ITEM_WEAR_NECK = MysteryLoader.C;
        private const long ITEM_WEAR_BODY = MysteryLoader.D;
        private const long ITEM_WEAR_HEAD = MysteryLoader.E;
        private const long ITEM_WEAR_LEGS = MysteryLoader.F;
        private const long ITEM_WEAR_FEET = MysteryLoader.G;
        private const long ITEM_WEAR_HANDS = MysteryLoader.H;
        private const long ITEM_WEAR_ARMS = MysteryLoader.I;
        private const long ITEM_WEAR_SHIELD = MysteryLoader.J;
        private const long ITEM_WEAR_ABOUT = MysteryLoader.K;
        private const long ITEM_WEAR_WAIST = MysteryLoader.L;
        private const long ITEM_WEAR_WRIST = MysteryLoader.M;
        private const long ITEM_WIELD = MysteryLoader.N;
        private const long ITEM_HOLD = MysteryLoader.O;
        private const long ITEM_WEAR_FLOAT = MysteryLoader.Q;
        private const long ITEM_WEAR_EAR = MysteryLoader.R;
        private const long ITEM_WEAR_EYES = MysteryLoader.S;

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
        private const long WEAPON_FLAMING = MysteryLoader.A;
        private const long WEAPON_FROST = MysteryLoader.B;
        private const long WEAPON_VAMPIRIC = MysteryLoader.C;
        private const long WEAPON_SHARP = MysteryLoader.D;
        private const long WEAPON_VORPAL = MysteryLoader.E;
        private const long WEAPON_TWO_HANDS = MysteryLoader.F;
        private const long WEAPON_SHOCKING = MysteryLoader.G;
        private const long WEAPON_POISON = MysteryLoader.H;
        private const long WEAPON_HOLY = MysteryLoader.I; // Added by SinaC 2001
        private const long WEAPON_WEIGHTED = MysteryLoader.J; // Added by SinaC 2001 for weighted equivalent to sharp for mace
        private const long WEAPON_NECROTISM = MysteryLoader.K; // Added by SinaC 2003

        // extra flags
        private const long ITEM_GLOW = MysteryLoader.A;
        private const long ITEM_HUM = MysteryLoader.B;
        private const long ITEM_DARK = MysteryLoader.C;
        private const long ITEM_STAY_DEATH = MysteryLoader.D; // Has replaced ITEM_LOCK with ITEM_STAY_DEATH, SinaC 2000
        private const long ITEM_EVIL = MysteryLoader.E;
        private const long ITEM_INVIS = MysteryLoader.F;
        private const long ITEM_MAGIC = MysteryLoader.G;
        private const long ITEM_NODROP = MysteryLoader.H;
        private const long ITEM_BLESS = MysteryLoader.I;
        private const long ITEM_ANTI_GOOD = MysteryLoader.J;
        private const long ITEM_ANTI_EVIL = MysteryLoader.K;
        private const long ITEM_ANTI_NEUTRAL = MysteryLoader.L;
        private const long ITEM_NOREMOVE = MysteryLoader.M;
        private const long ITEM_INVENTORY = MysteryLoader.N;
        private const long ITEM_NOPURGE = MysteryLoader.O;
        private const long ITEM_ROT_DEATH = MysteryLoader.P;
        private const long ITEM_VIS_DEATH = MysteryLoader.Q;
        private const long ITEM_UNIQUE = MysteryLoader.S; // replace NON_METAL with is implemented thru material table in Mystery Added by SinaC 2001 for unique item
        private const long ITEM_NOLOCATE = MysteryLoader.T;
        private const long ITEM_MELT_DROP = MysteryLoader.U;
        private const long ITEM_HAD_TIMER = MysteryLoader.V;
        private const long ITEM_SELL_EXTRACT = MysteryLoader.W;
        private const long ITEM_NOSAC = MysteryLoader.X; // Added by SinaC 2001, was in wear flags before
        private const long ITEM_BURN_PROOF = MysteryLoader.Y;
        private const long ITEM_NOUNCURSE = MysteryLoader.Z;
        private const long ITEM_NOIDENT = MysteryLoader.aa; // NoIdent item SinaC 2000
        private const long ITEM_NOCOND = MysteryLoader.bb; // No condition item SinaC 2001

        // portal flags
        private const long GATE_NORMAL_EXIT = MysteryLoader.A;
        private const long GATE_NOCURSE = MysteryLoader.B;
        private const long GATE_GOWITH = MysteryLoader.C;
        private const long GATE_BUGGY = MysteryLoader.D;
        private const long GATE_RANDOM = MysteryLoader.E;

        // container flags
        private const long CONT_CLOSEABLE = 1;
        private const long CONT_PICKPROOF = 2;
        private const long CONT_CLOSED = 4;
        private const long CONT_LOCKED = 8;
        private const long CONT_PUT_ON = 16;

        // furniture flags
        private const long STAND_AT = MysteryLoader.A;
        private const long STAND_ON = MysteryLoader.B;
        private const long STAND_IN = MysteryLoader.C;
        private const long SIT_AT = MysteryLoader.D;
        private const long SIT_ON = MysteryLoader.E;
        private const long SIT_IN = MysteryLoader.F;
        private const long REST_AT = MysteryLoader.G;
        private const long REST_ON = MysteryLoader.H;
        private const long REST_IN = MysteryLoader.I;
        private const long SLEEP_AT = MysteryLoader.J;
        private const long SLEEP_ON = MysteryLoader.K;
        private const long SLEEP_IN = MysteryLoader.L;
        private const long PUT_AT = MysteryLoader.M;
        private const long PUT_ON = MysteryLoader.N;
        private const long PUT_IN = MysteryLoader.O;
        private const long PUT_INSIDE = MysteryLoader.P;

        #endregion

        #region Mobile

        private CharacterBlueprintBase ConvertMobile(MobileData mobileData)
        {
            if (_characterBlueprints.Any(x => x.Id == mobileData.VNum))
                RaiseConvertException("Duplicate mobile Id {0}", mobileData.VNum);

            SchoolTypes schoolType = SchoolTypes.None;
            string damageNoun = mobileData.DamageType;
            (string name, string noun, int damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == mobileData.DamageType);
            if (!attackTableEntry.Equals(default))
            {
                schoolType = ConvertDamageType(attackTableEntry.damType, $"mob {mobileData.VNum}");
                damageNoun = attackTableEntry.noun;
            }

            (OffensiveFlags offensiveFlags, AssistFlags assistFlags) offAssistFlags = ConvertMysteryOffensiveFlags(mobileData.OffFlags);

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
                    CharacterFlags = ConvertMysteryCharacterFlags(mobileData.AffectedBy),
                    ActFlags = ConvertMysteryActFlags(mobileData.Act),
                    OffensiveFlags = offAssistFlags.offensiveFlags,
                    AssistFlags = offAssistFlags.assistFlags,
                    Immunities = ConvertMysteryIRV(mobileData.ImmFlags),
                    Resistances = ConvertMysteryIRV(mobileData.ResFlags),
                    Vulnerabilities = ConvertMysteryIRV(mobileData.VulnFlags),
                    Race = mobileData.Race,
                    BodyForms = ConvertBodyForms(mobileData.Form),
                    BodyParts = ConvertBodyParts(mobileData.Parts),
                    //Class = mobileData.Classes
                };
            else
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
                    CharacterFlags = ConvertMysteryCharacterFlags(mobileData.AffectedBy),
                    ActFlags = ConvertMysteryActFlags(mobileData.Act),
                    OffensiveFlags = offAssistFlags.offensiveFlags,
                    AssistFlags = offAssistFlags.assistFlags,
                    Immunities = ConvertMysteryIRV(mobileData.ImmFlags),
                    Resistances = ConvertMysteryIRV(mobileData.ResFlags),
                    Vulnerabilities = ConvertMysteryIRV(mobileData.VulnFlags),
                    Race = mobileData.Race,
                    BodyForms = ConvertBodyForms(mobileData.Form),
                    BodyParts = ConvertBodyParts(mobileData.Parts),
                    //Class = mobileData.Classes
                    //
                    BuyBlueprintTypes = ConvertBuyTypes(mobileData.Shop).ToList(),
                    ProfitBuy = mobileData.Shop.ProfitBuy,
                    ProfitSell = mobileData.Shop.ProfitSell,
                    OpenHour = mobileData.Shop.OpenHour,
                    CloseHour = mobileData.Shop.CloseHour,
                };
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
                    Log.Default.WriteLine(LogLevels.Error, "Invalid size {0} for mob {1}", mobileData.Size, mobileData.VNum);
                    return Sizes.Medium;
            }
        }

        private IRVFlags ConvertMysteryIRV(long value)
        {
            IRVFlags flags = 0;
            if (IsSet(value, IRV_SUMMON)) flags |= IRVFlags.Summon;
            if (IsSet(value, IRV_CHARM)) flags |= IRVFlags.Charm;
            if (IsSet(value, IRV_MAGIC)) flags |= IRVFlags.Magic;
            if (IsSet(value, IRV_WEAPON)) flags |= IRVFlags.Weapon;
            if (IsSet(value, IRV_BASH)) flags |= IRVFlags.Bash;
            if (IsSet(value, IRV_PIERCE)) flags |= IRVFlags.Pierce;
            if (IsSet(value, IRV_SLASH)) flags |= IRVFlags.Slash;
            if (IsSet(value, IRV_FIRE)) flags |= IRVFlags.Fire;
            if (IsSet(value, IRV_COLD)) flags |= IRVFlags.Cold;
            if (IsSet(value, IRV_LIGHTNING)) flags |= IRVFlags.Lightning;
            if (IsSet(value, IRV_ACID)) flags |= IRVFlags.Acid;
            if (IsSet(value, IRV_POISON)) flags |= IRVFlags.Poison;
            if (IsSet(value, IRV_NEGATIVE)) flags |= IRVFlags.Negative;
            if (IsSet(value, IRV_HOLY)) flags |= IRVFlags.Holy;
            if (IsSet(value, IRV_ENERGY)) flags |= IRVFlags.Energy;
            if (IsSet(value, IRV_MENTAL)) flags |= IRVFlags.Mental;
            if (IsSet(value, IRV_DISEASE)) flags |= IRVFlags.Disease;
            if (IsSet(value, IRV_DROWNING)) flags |= IRVFlags.Drowning;
            if (IsSet(value, IRV_LIGHT)) flags |= IRVFlags.Light;
            if (IsSet(value, IRV_SOUND)) flags |= IRVFlags.Sound;
            if (IsSet(value, IRV_WOOD)) flags |= IRVFlags.Wood;
            if (IsSet(value, IRV_SILVER)) flags |= IRVFlags.Silver;
            if (IsSet(value, IRV_IRON)) flags |= IRVFlags.Iron;
            // IRV_DAYLIGHT
            // IRV_EARTH
            // IRV_WEAKEN

            return flags;
        }

        private CharacterFlags ConvertMysteryCharacterFlags(long affectedBy)
        {
            CharacterFlags flags = CharacterFlags.None;
            if (IsSet(affectedBy, AFF_BLIND)) flags |= CharacterFlags.Blind;
            if (IsSet(affectedBy, AFF_INVISIBLE)) flags |= CharacterFlags.Invisible;
            if (IsSet(affectedBy, AFF_DETECT_EVIL)) flags |= CharacterFlags.DetectEvil;
            if (IsSet(affectedBy, AFF_DETECT_INVIS)) flags |= CharacterFlags.DetectInvis;
            if (IsSet(affectedBy, AFF_DETECT_MAGIC)) flags |= CharacterFlags.DetectMagic;
            if (IsSet(affectedBy, AFF_DETECT_HIDDEN)) flags |= CharacterFlags.DetectHidden;
            if (IsSet(affectedBy, AFF_DETECT_GOOD)) flags |= CharacterFlags.DetectGood;
            if (IsSet(affectedBy, AFF_SANCTUARY)) flags |= CharacterFlags.Sanctuary;
            if (IsSet(affectedBy, AFF_FAERIE_FIRE)) flags |= CharacterFlags.FaerieFire;
            if (IsSet(affectedBy, AFF_INFRARED)) flags |= CharacterFlags.Infrared;
            if (IsSet(affectedBy, AFF_CURSE)) flags |= CharacterFlags.Curse;
            // AFF_ROOTED
            if (IsSet(affectedBy, AFF_POISON)) flags |= CharacterFlags.Poison;
            if (IsSet(affectedBy, AFF_PROTECT_EVIL)) flags |= CharacterFlags.ProtectEvil;
            if (IsSet(affectedBy, AFF_PROTECT_GOOD)) flags |= CharacterFlags.ProtectGood;
            if (IsSet(affectedBy, AFF_SNEAK)) flags |= CharacterFlags.Sneak;
            if (IsSet(affectedBy, AFF_HIDE)) flags |= CharacterFlags.Hide;
            if (IsSet(affectedBy, AFF_SLEEP)) flags |= CharacterFlags.Sleep;
            if (IsSet(affectedBy, AFF_CHARM)) flags |= CharacterFlags.Charm;
            if (IsSet(affectedBy, AFF_FLYING)) flags |= CharacterFlags.Flying;
            if (IsSet(affectedBy, AFF_PASS_DOOR)) flags |= CharacterFlags.PassDoor;
            if (IsSet(affectedBy, AFF_HASTE)) flags |= CharacterFlags.Haste;
            if (IsSet(affectedBy, AFF_CALM)) flags |= CharacterFlags.Calm;
            if (IsSet(affectedBy, AFF_PLAGUE)) flags |= CharacterFlags.Plague;
            if (IsSet(affectedBy, AFF_WEAKEN)) flags |= CharacterFlags.Weaken;
            if (IsSet(affectedBy, AFF_DARK_VISION)) flags |= CharacterFlags.DarkVision;
            if (IsSet(affectedBy, AFF_BERSERK)) flags |= CharacterFlags.Berserk;
            if (IsSet(affectedBy, AFF_SWIM)) flags |= CharacterFlags.Swim;
            if (IsSet(affectedBy, AFF_REGENERATION)) flags |= CharacterFlags.Regeneration;
            if (IsSet(affectedBy, AFF_SLOW)) flags |= CharacterFlags.Slow;
            // AFF_SILENCE
            // AFF2_WALK_ON_WATER
            // AFF2_WATER_BREATH
            // AFF2_DETECT_EXITS
            // AFF2_MAGIC_MIRROR
            // AFF2_FAERIE_FOG
            // AFF2_NOEQUIPMENT
            // AFF2_FREE_MOVEMENT
            // AFF2_INCREASED_CASTING
            // AFF2_NOSPELL
            // AFF2_NECROTISM
            // AFF2_HIGHER_MAGIC_ATTRIBUTES
            // AFF2_CONFUSION

            return flags;
        }

        private ActFlags ConvertMysteryActFlags(long act)
        {
            ActFlags flags = ActFlags.None;

            //ACT_IS_NPC not used
            if (IsSet(act, ACT_SENTINEL)) flags |= ActFlags.Sentinel;
            if (IsSet(act, ACT_SCAVENGER)) flags |= ActFlags.Scavenger;
            //ACT_AWARE
            if (IsSet(act, ACT_AGGRESSIVE)) flags |= ActFlags.Aggressive;
            if (IsSet(act, ACT_STAY_AREA)) flags |= ActFlags.StayArea;
            if (IsSet(act, ACT_WIMPY)) flags |= ActFlags.Wimpy;
            if (IsSet(act, ACT_PET)) flags |= ActFlags.Pet;
            if (IsSet(act, ACT_TRAIN)) flags |= ActFlags.Train;
            if (IsSet(act, ACT_PRACTICE)) flags |= ActFlags.Practice;
            //ACT_FREE_WANDER
            //ACT_MOUNTABLE
            //ACT_IS_MOUNTED
            if (IsSet(act, ACT_UNDEAD)) flags |= ActFlags.Undead;
            //ACT_NOSLEEP
            if (IsSet(act, ACT_CLERIC)) flags |= ActFlags.Cleric;
            if (IsSet(act, ACT_MAGE)) flags |= ActFlags.Mage;
            if (IsSet(act, ACT_THIEF)) flags |= ActFlags.Thief;
            if (IsSet(act, ACT_WARRIOR)) flags |= ActFlags.Warrior;
            if (IsSet(act, ACT_NOALIGN)) flags |= ActFlags.NoAlign;
            if (IsSet(act, ACT_NOPURGE)) flags |= ActFlags.NoPurge;
            if (IsSet(act, ACT_OUTDOORS)) flags |= ActFlags.Outdoors;
            if (IsSet(act, ACT_INDOORS)) flags |= ActFlags.Indoors;
            //ACT_CREATED
            if (IsSet(act, ACT_IS_HEALER)) flags |= ActFlags.IsHealer;
            if (IsSet(act, ACT_GAIN)) flags |= ActFlags.Gain;
            if (IsSet(act, ACT_UPDATE_ALWAYS)) flags |= ActFlags.UpdateAlways;
            //ACT_IS_CHANGER
            //ACT_IS_SAFE
            return flags;
        }

        private (OffensiveFlags, AssistFlags) ConvertMysteryOffensiveFlags(long input)
        {
            OffensiveFlags off = OffensiveFlags.None;
            if (IsSet(input, OFF_AREA_ATTACK)) off |= OffensiveFlags.AreaAttack;
            if (IsSet(input, OFF_BACKSTAB)) off |= OffensiveFlags.Backstab;
            if (IsSet(input, OFF_BASH)) off |= OffensiveFlags.Bash;
            if (IsSet(input, OFF_BERSERK)) off |= OffensiveFlags.Berserk;
            if (IsSet(input, OFF_DISARM)) off |= OffensiveFlags.Disarm;
            if (IsSet(input, OFF_DODGE)) off |= OffensiveFlags.Dodge;
            if (IsSet(input, OFF_FADE)) off |= OffensiveFlags.Fade;
            if (IsSet(input, OFF_FAST)) off |= OffensiveFlags.Fast;
            if (IsSet(input, OFF_KICK)) off |= OffensiveFlags.Kick;
            if (IsSet(input, OFF_KICK_DIRT)) off |= OffensiveFlags.DirtKick;
            if (IsSet(input, OFF_PARRY)) off |= OffensiveFlags.Parry;
            if (IsSet(input, OFF_RESCUE)) off |= OffensiveFlags.Rescue;
            if (IsSet(input, OFF_TAIL)) off |= OffensiveFlags.Tail;
            if (IsSet(input, OFF_TRIP)) off |= OffensiveFlags.Trip;
            if (IsSet(input, OFF_CRUSH)) off |= OffensiveFlags.Crush;
            //OFF_COUNTER
            //OFF_BITE

            AssistFlags assist = AssistFlags.None;
            if (IsSet(input, ASSIST_ALL)) assist |= AssistFlags.All;
            if (IsSet(input, ASSIST_ALIGN)) assist |= AssistFlags.Align;
            if (IsSet(input, ASSIST_RACE)) assist |= AssistFlags.Race;
            if (IsSet(input, ASSIST_PLAYERS)) assist |= AssistFlags.Players;
            if (IsSet(input, ASSIST_GUARD)) assist |= AssistFlags.Guard;
            if (IsSet(input, ASSIST_VNUM)) assist |= AssistFlags.Vnum;

            return (off, assist);
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
                    case ITEM_MAP: yield return typeof(ItemMapBlueprint); break;
                    case ITEM_PORTAL: yield return typeof(ItemPortalBlueprint); break;
                    case ITEM_WARP_STONE: yield return typeof(ItemWarpStoneBlueprint); break;
                    case ITEM_COMPONENT:
                        Log.Default.WriteLine(LogLevels.Warning, "Invalid buy type {0} for mob {1}", buyType, shopData.Keeper);
                        break;
                    case ITEM_GEM: yield return typeof(ItemGemBlueprint); break;
                    case ITEM_JEWELRY: yield return typeof(ItemJewelryBlueprint); break;
                    case ITEM_INSTRUMENT:
                    case ITEM_WINDOW:
                    case ITEM_TEMPLATE:
                    case ITEM_SADDLE:
                    case ITEM_ROPE:
                    default:
                        Log.Default.WriteLine(LogLevels.Warning, "Invalid buy type {0} for mob {1}", buyType, shopData.Keeper);
                        break;
                }
            }
        }


        private BodyForms ConvertBodyForms(long input)
        {
            BodyForms forms = BodyForms.None;

            if (IsSet(input, FORM_EDIBLE)) forms |= BodyForms.Edible;
            if (IsSet(input, FORM_POISON)) forms |= BodyForms.Poison;
            if (IsSet(input, FORM_MAGICAL)) forms |= BodyForms.Magical;
            if (IsSet(input, FORM_INSTANT_DECAY)) forms |= BodyForms.InstantDecay;
            if (IsSet(input, FORM_OTHER)) forms |= BodyForms.Other;
            if (IsSet(input, FORM_ANIMAL)) forms |= BodyForms.Animal;
            if (IsSet(input, FORM_SENTIENT)) forms |= BodyForms.Sentient;
            if (IsSet(input, FORM_UNDEAD)) forms |= BodyForms.Undead;
            if (IsSet(input, FORM_CONSTRUCT)) forms |= BodyForms.Construct;
            if (IsSet(input, FORM_MIST)) forms |= BodyForms.Mist;
            if (IsSet(input, FORM_INTANGIBLE)) forms |= BodyForms.Intangible;
            if (IsSet(input, FORM_BIPED)) forms |= BodyForms.Biped;
            if (IsSet(input, FORM_CENTAUR)) forms |= BodyForms.Centaur;
            if (IsSet(input, FORM_INSECT)) forms |= BodyForms.Insect;
            if (IsSet(input, FORM_SPIDER)) forms |= BodyForms.Spider;
            if (IsSet(input, FORM_CRUSTACEAN)) forms |= BodyForms.Crustacean;
            if (IsSet(input, FORM_WORM)) forms |= BodyForms.Worm;
            if (IsSet(input, FORM_BLOB)) forms |= BodyForms.Blob;
            if (IsSet(input, FORM_MAMMAL)) forms |= BodyForms.Mammal;
            if (IsSet(input, FORM_BIRD)) forms |= BodyForms.Bird;
            if (IsSet(input, FORM_REPTILE)) forms |= BodyForms.Reptile;
            if (IsSet(input, FORM_SNAKE)) forms |= BodyForms.Snake;
            if (IsSet(input, FORM_DRAGON)) forms |= BodyForms.Dragon;
            if (IsSet(input, FORM_AMPHIBIAN)) forms |= BodyForms.Amphibian;
            if (IsSet(input, FORM_FISH)) forms |= BodyForms.Fish;
            if (IsSet(input, FORM_COLD_BLOOD)) forms |= BodyForms.ColdBlood;
            //FORM_FUR
            //FORM_FOUR_ARMS

            return forms;
        }

        private BodyParts ConvertBodyParts(long input)
        {
            BodyParts parts = BodyParts.None;

            if (IsSet(input, PART_HEAD)) parts |= BodyParts.Head;
            if (IsSet(input, PART_ARMS)) parts |= BodyParts.Arms;
            if (IsSet(input, PART_LEGS)) parts |= BodyParts.Legs;
            if (IsSet(input, PART_HEART)) parts |= BodyParts.Heart;
            if (IsSet(input, PART_BRAINS)) parts |= BodyParts.Brains;
            if (IsSet(input, PART_GUTS)) parts |= BodyParts.Guts;
            if (IsSet(input, PART_HANDS)) parts |= BodyParts.Hands;
            if (IsSet(input, PART_FEET)) parts |= BodyParts.Feet;
            if (IsSet(input, PART_FINGERS)) parts |= BodyParts.Fingers;
            if (IsSet(input, PART_EAR)) parts |= BodyParts.Ear;
            if (IsSet(input, PART_EYE)) parts |= BodyParts.Eye;
            if (IsSet(input, PART_LONG_TONGUE)) parts |= BodyParts.LongTongue;
            if (IsSet(input, PART_EYESTALKS)) parts |= BodyParts.Eyestalks;
            if (IsSet(input, PART_TENTACLES)) parts |= BodyParts.Tentacles;
            if (IsSet(input, PART_FINS)) parts |= BodyParts.Fins;
            if (IsSet(input, PART_WINGS)) parts |= BodyParts.Wings;
            if (IsSet(input, PART_TAIL)) parts |= BodyParts.Tail;
            // PART_BODY
            if (IsSet(input, PART_CLAWS)) parts |= BodyParts.Claws;
            if (IsSet(input, PART_FANGS)) parts |= BodyParts.Fangs;
            if (IsSet(input, PART_HORNS)) parts |= BodyParts.Horns;
            if (IsSet(input, PART_SCALES)) parts |= BodyParts.Scales;
            if (IsSet(input, PART_TUSKS)) parts |= BodyParts.Tusks;

            return parts;
        }

        // Immunites, Resistances, Vulnerabilities
        private const long IRV_SUMMON               = MysteryLoader.A;
        private const long IRV_CHARM                = MysteryLoader.B;
        private const long IRV_MAGIC                = MysteryLoader.C;
        private const long IRV_WEAPON               = MysteryLoader.D;
        private const long IRV_BASH                 = MysteryLoader.E;
        private const long IRV_PIERCE               = MysteryLoader.F;
        private const long IRV_SLASH                = MysteryLoader.G;
        private const long IRV_FIRE                 = MysteryLoader.H;
        private const long IRV_COLD                 = MysteryLoader.I;
        private const long IRV_LIGHTNING            = MysteryLoader.J;
        private const long IRV_ACID                 = MysteryLoader.K;
        private const long IRV_POISON               = MysteryLoader.L;
        private const long IRV_NEGATIVE             = MysteryLoader.M;
        private const long IRV_HOLY                 = MysteryLoader.N;
        private const long IRV_ENERGY               = MysteryLoader.O;
        private const long IRV_MENTAL               = MysteryLoader.P;
        private const long IRV_DISEASE              = MysteryLoader.Q;
        private const long IRV_DROWNING             = MysteryLoader.R;
        private const long IRV_LIGHT		 = MysteryLoader.S;
        private const long IRV_SOUND		 = MysteryLoader.T;
        private const long IRV_PARALYSIS            = MysteryLoader.V;
        private const long IRV_WOOD                 = MysteryLoader.X;
        private const long IRV_SILVER               = MysteryLoader.Y;
        private const long IRV_IRON                 = MysteryLoader.Z;
        private const long IRV_DAYLIGHT             = MysteryLoader.aa; // Added by SinaC 2001
        private const long IRV_EARTH                = MysteryLoader.bb; // Added by SinaC 2003
        private const long IRV_WEAKEN               = MysteryLoader.cc; // Added by SinaC 2003

        // Affected by
        private const long AFF_BLIND = MysteryLoader.A;
        private const long AFF_INVISIBLE = MysteryLoader.B;
        private const long AFF_DETECT_EVIL = MysteryLoader.C;
        private const long AFF_DETECT_INVIS = MysteryLoader.D;
        private const long AFF_DETECT_MAGIC = MysteryLoader.E;
        private const long AFF_DETECT_HIDDEN = MysteryLoader.F;
        private const long AFF_DETECT_GOOD = MysteryLoader.G;
        private const long AFF_SANCTUARY = MysteryLoader.H;
        private const long AFF_FAERIE_FIRE = MysteryLoader.I;
        private const long AFF_INFRARED = MysteryLoader.J;
        private const long AFF_CURSE = MysteryLoader.K;
        private const long AFF_ROOTED = MysteryLoader.L;
        private const long AFF_POISON = MysteryLoader.M;
        private const long AFF_PROTECT_EVIL = MysteryLoader.N;
        private const long AFF_PROTECT_GOOD = MysteryLoader.O;
        private const long AFF_SNEAK = MysteryLoader.P;
        private const long AFF_HIDE = MysteryLoader.Q;
        private const long AFF_SLEEP = MysteryLoader.R;
        private const long AFF_CHARM = MysteryLoader.S;
        private const long AFF_FLYING = MysteryLoader.T;
        private const long AFF_PASS_DOOR = MysteryLoader.U;
        private const long AFF_HASTE = MysteryLoader.V;
        private const long AFF_CALM = MysteryLoader.W;
        private const long AFF_PLAGUE = MysteryLoader.X;
        private const long AFF_WEAKEN = MysteryLoader.Y;
        private const long AFF_DARK_VISION = MysteryLoader.Z;
        private const long AFF_BERSERK = MysteryLoader.aa;
        private const long AFF_SWIM = MysteryLoader.bb;
        private const long AFF_REGENERATION = MysteryLoader.cc;
        private const long AFF_SLOW = MysteryLoader.dd;
        private const long AFF_SILENCE = MysteryLoader.ee; // SinaC 2000

        // Added by SinaC 2001
        private const long AFF2_WALK_ON_WATER = MysteryLoader.A;
        private const long AFF2_WATER_BREATH = MysteryLoader.B;
        private const long AFF2_DETECT_EXITS = MysteryLoader.C;
        private const long AFF2_MAGIC_MIRROR = MysteryLoader.D;
        private const long AFF2_FAERIE_FOG = MysteryLoader.E;
        private const long AFF2_NOEQUIPMENT = MysteryLoader.F;
        // Added by SinaC 2003
        private const long AFF2_FREE_MOVEMENT = MysteryLoader.G;
        private const long AFF2_INCREASED_CASTING = MysteryLoader.H;
        private const long AFF2_NOSPELL = MysteryLoader.I;
        private const long AFF2_NECROTISM = MysteryLoader.J;
        private const long AFF2_HIGHER_MAGIC_ATTRIBUTES = MysteryLoader.K;
        private const long AFF2_CONFUSION = MysteryLoader.L;


        // Act flags
        private const long ACT_IS_NPC = MysteryLoader.A;
        private const long ACT_SENTINEL = MysteryLoader.B;
        private const long ACT_SCAVENGER = MysteryLoader.C;
        private const long ACT_AWARE = MysteryLoader.E;             // can't be backstab
        private const long ACT_AGGRESSIVE = MysteryLoader.F;
        private const long ACT_STAY_AREA = MysteryLoader.G;
        private const long ACT_WIMPY = MysteryLoader.H;
        private const long ACT_PET = MysteryLoader.I;
        private const long ACT_TRAIN = MysteryLoader.J;
        private const long ACT_PRACTICE = MysteryLoader.K;
        private const long ACT_FREE_WANDER = MysteryLoader.L; // can leave an area without being extract, SinaC 2001
        private const long ACT_MOUNTABLE = MysteryLoader.M;
        private const long ACT_IS_MOUNTED = MysteryLoader.N;
        private const long ACT_UNDEAD = MysteryLoader.O;
        private const long ACT_NOSLEEP = MysteryLoader.P;
        private const long ACT_CLERIC = MysteryLoader.Q;
        private const long ACT_MAGE = MysteryLoader.R;
        private const long ACT_THIEF = MysteryLoader.S;
        private const long ACT_WARRIOR = MysteryLoader.T;
        private const long ACT_NOALIGN = MysteryLoader.U;
        private const long ACT_NOPURGE = MysteryLoader.V;
        private const long ACT_OUTDOORS = MysteryLoader.W;
        private const long ACT_INDOORS = MysteryLoader.Y;
        private const long ACT_CREATED = MysteryLoader.Z;
        private const long ACT_IS_HEALER = MysteryLoader.aa;
        private const long ACT_GAIN = MysteryLoader.bb;
        private const long ACT_UPDATE_ALWAYS = MysteryLoader.cc;
        private const long ACT_IS_CHANGER = MysteryLoader.dd;
        private const long ACT_IS_SAFE = MysteryLoader.ee;

        // Offensive flags
        private const long OFF_AREA_ATTACK = MysteryLoader.A;
        private const long OFF_BACKSTAB = MysteryLoader.B;
        private const long OFF_BASH = MysteryLoader.C;
        private const long OFF_BERSERK = MysteryLoader.D;
        private const long OFF_DISARM = MysteryLoader.E;
        private const long OFF_DODGE = MysteryLoader.F;
        private const long OFF_FADE = MysteryLoader.G;
        private const long OFF_FAST = MysteryLoader.H;
        private const long OFF_KICK = MysteryLoader.I;
        private const long OFF_KICK_DIRT = MysteryLoader.J;
        private const long OFF_PARRY = MysteryLoader.K;
        private const long OFF_RESCUE = MysteryLoader.L;
        private const long OFF_TAIL = MysteryLoader.M;
        private const long OFF_TRIP = MysteryLoader.N;
        private const long OFF_CRUSH = MysteryLoader.O;
        private const long ASSIST_ALL = MysteryLoader.P;
        private const long ASSIST_ALIGN = MysteryLoader.Q;
        private const long ASSIST_RACE = MysteryLoader.R;
        private const long ASSIST_PLAYERS = MysteryLoader.S;
        private const long ASSIST_GUARD = MysteryLoader.T;
        private const long ASSIST_VNUM = MysteryLoader.U;
        // Added by SinaC 2000 to add some fun :)))) counter-attack for mobiles
        private const long OFF_COUNTER = MysteryLoader.V;
        private const long OFF_BITE = MysteryLoader.W;

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
        private const int ITEM_MAP = 28;
        private const int ITEM_PORTAL = 29;
        private const int ITEM_WARP_STONE = 30;
        private const int ITEM_COMPONENT = 31;
        private const int ITEM_GEM = 32;
        private const int ITEM_JEWELRY = 33;
        private const int ITEM_INSTRUMENT = 35;
        private const int ITEM_WINDOW = 37;
        private const int ITEM_TEMPLATE = 40;
        private const int ITEM_SADDLE = 41;
        private const int ITEM_ROPE = 42;

        // body form
        private const long FORM_EDIBLE = MysteryLoader.A;
        private const long FORM_POISON = MysteryLoader.B;
        private const long FORM_MAGICAL = MysteryLoader.C;
        private const long FORM_INSTANT_DECAY = MysteryLoader.D;
        private const long FORM_OTHER = MysteryLoader.E;  /* defined by material bit */
        // actual form
        private const long FORM_ANIMAL = MysteryLoader.G;
        private const long FORM_SENTIENT = MysteryLoader.H;
        private const long FORM_UNDEAD = MysteryLoader.I;
        private const long FORM_CONSTRUCT = MysteryLoader.J;
        private const long FORM_MIST = MysteryLoader.K;
        private const long FORM_INTANGIBLE = MysteryLoader.L;
        private const long FORM_BIPED = MysteryLoader.M;
        private const long FORM_CENTAUR = MysteryLoader.N;
        private const long FORM_INSECT = MysteryLoader.O;
        private const long FORM_SPIDER = MysteryLoader.P;
        private const long FORM_CRUSTACEAN = MysteryLoader.Q;
        private const long FORM_WORM = MysteryLoader.R;
        private const long FORM_BLOB = MysteryLoader.S;
        private const long FORM_MAMMAL = MysteryLoader.V;
        private const long FORM_BIRD = MysteryLoader.W;
        private const long FORM_REPTILE = MysteryLoader.X;
        private const long FORM_SNAKE = MysteryLoader.Y;
        private const long FORM_DRAGON = MysteryLoader.Z;
        private const long FORM_AMPHIBIAN = MysteryLoader.aa;
        private const long FORM_FISH = MysteryLoader.bb;
        private const long FORM_COLD_BLOOD = MysteryLoader.cc;
        // Added by SinaC 2001
        private const long FORM_FUR = MysteryLoader.dd;
        // SinaC 2003
        private const long FORM_FOUR_ARMS = MysteryLoader.ee;

        // body parts
        private const long PART_HEAD = MysteryLoader.A;
        private const long PART_ARMS = MysteryLoader.B;
        private const long PART_LEGS = MysteryLoader.C;
        private const long PART_HEART = MysteryLoader.D;
        private const long PART_BRAINS = MysteryLoader.E;
        private const long PART_GUTS = MysteryLoader.F;
        private const long PART_HANDS = MysteryLoader.G;
        private const long PART_FEET = MysteryLoader.H;
        private const long PART_FINGERS = MysteryLoader.I;
        private const long PART_EAR = MysteryLoader.J;
        private const long PART_EYE = MysteryLoader.K;
        private const long PART_LONG_TONGUE = MysteryLoader.L;
        private const long PART_EYESTALKS = MysteryLoader.M;
        private const long PART_TENTACLES = MysteryLoader.N;
        private const long PART_FINS = MysteryLoader.O;
        private const long PART_WINGS = MysteryLoader.P;
        private const long PART_TAIL = MysteryLoader.Q;
        // PART_BODY: SinaC 2003  set for almost every races except blob and things we don't want to see with armor
        private const long PART_BODY = MysteryLoader.R;
        // for combat
        private const long PART_CLAWS = MysteryLoader.U;
        private const long PART_FANGS = MysteryLoader.V;
        private const long PART_HORNS = MysteryLoader.W;
        private const long PART_SCALES = MysteryLoader.X;
        private const long PART_TUSKS = MysteryLoader.Y;

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
                // DAYLIGHT, EARTH, WEAKEN
            }

            Log.Default.WriteLine(LogLevels.Warning, "Unknown damage type {0} for {1}", damageType, errorMsg);
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
        // Added by SinaC 2001
        private const int DAM_DAYLIGHT = 20;
        // Added by SinaC 2003
        private const int DAM_EARTH = 21;
        private const int DAM_WEAKEN = 22;

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
            string message = string.Format(format, parameters);
            Log.Default.WriteLine(LogLevels.Error, message);
            throw new MysteryConvertException(message);
        }

        //
        private string RemoveCommentIfAny(string filename)
        {
            int index = filename.IndexOf("*", StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
                return filename.Remove(index).Trim();
            return filename;
        }
    }
}
