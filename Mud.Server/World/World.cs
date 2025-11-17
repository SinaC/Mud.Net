using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.World;

public class World : IWorld
{
    private IRandomManager RandomManager { get; }
    private ISettings Settings { get; }
    private IItemManager ItemManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IRoomManager RoomManager { get; }
    private IAreaManager AreaManager { get; }
    private IResetManager ResetManager { get; }

    private readonly List<TreasureTable<int>> _treasureTables;

    public World(IRandomManager randomManager, ISettings settings, IItemManager itemManager, ICharacterManager characterManager, IRoomManager roomManager, IAreaManager areaManager, IResetManager resetManager)
    {
        RandomManager = randomManager;
        Settings = settings;
        ItemManager = itemManager;
        CharacterManager = characterManager;
        RoomManager = roomManager;
        AreaManager = areaManager;
        ResetManager = resetManager;

        _treasureTables = [];
    }


    public IReadOnlyCollection<TreasureTable<int>> TreasureTables => _treasureTables;

    public void AddTreasureTable(TreasureTable<int> table)
    {
        // TODO: check if already exists ?
        _treasureTables.Add(table);
    }

    public void FixWorld()
    {
        FixItems();
        FixResets();
    }

    public void ResetWorld()
    {
        foreach (IArea area in AreaManager.Areas)
        {
            // TODO: handle age + at load time, force age to arbitrary high value to ensure reset are computed
            //if (area.PlayableCharacters.Any())
            {
                ResetManager.ResetArea(area);
            }
        }
    }

    public void Cleanup() // remove invalid entities
    {
        RoomManager.Cleanup();
        CharacterManager.Cleanup();
        ItemManager.Cleanup();
    }

    private void FixItems()
    {
        Log.Default.WriteLine(LogLevels.Info, "Fixing items");
        foreach (var itemBlueprint in ItemManager.ItemBlueprints.OrderBy(x => x.Id))
        {
            switch (itemBlueprint)
            {
                case ItemLightBlueprint _:
                    if (itemBlueprint.WearLocation != WearLocations.Light)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Light {0} has wear location {1} -> set wear location to Light", itemBlueprint.Id, itemBlueprint.WearLocation);
                        itemBlueprint.WearLocation = WearLocations.Light;
                    }

                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    if (itemBlueprint.WearLocation != WearLocations.Wield && itemBlueprint.WearLocation != WearLocations.Wield2H)
                    {
                        var newWearLocation = WearLocations.Wield;
                        if (weaponBlueprint.Flags.IsSet("TwoHands"))
                            newWearLocation = WearLocations.Wield2H;
                        Log.Default.WriteLine(LogLevels.Error, "Weapon {0} has wear location {1} -> set wear location to {2}", itemBlueprint.Id, itemBlueprint.WearLocation, newWearLocation);
                        itemBlueprint.WearLocation = newWearLocation;
                    }

                    break;
                case ItemShieldBlueprint _:
                    if (itemBlueprint.WearLocation != WearLocations.Shield)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Shield {0} has wear location {1} -> set wear location to Shield", itemBlueprint.Id, itemBlueprint.WearLocation);
                        itemBlueprint.WearLocation = WearLocations.Shield;
                    }

                    break;
                case ItemStaffBlueprint _:
                case ItemWandBlueprint _:
                    if (itemBlueprint.WearLocation != WearLocations.Hold)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "{0} {1} has wear location {2} -> set wear location to Hold", itemBlueprint.ItemType(), itemBlueprint.Id, itemBlueprint.WearLocation);
                        itemBlueprint.WearLocation = WearLocations.Hold;
                    }

                    break;
            }
        }

        Log.Default.WriteLine(LogLevels.Info, "items fixed");
    }

    private void FixResets()
    {
        Log.Default.WriteLine(LogLevels.Info, "Fixing resets");

        // Global count is used to check global limit to 0
        Dictionary<int, int> characterResetGlobalCountById = [];
        Dictionary<int, int> itemResetCountGlobalById = [];

        foreach (var room in RoomManager.Rooms.Where(x => x.Blueprint.Resets?.Count > 0).OrderBy(x => x.Blueprint.Id))
        {
            Dictionary<int, int> characterResetCountById = [];
            Dictionary<int, int> itemResetCountById = [];

            // Count to check local limit  TODO: local limit is relative to container   example in dwarven.are Room 6534: item 6506 found with reset O and reset E -> 2 different containers
            foreach (ResetBase reset in room.Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset:
                        characterResetCountById.Increment(characterReset.CharacterId);
                        characterResetGlobalCountById.Increment(characterReset.CharacterId);
                        break;
                    case ItemInRoomReset itemInRoomReset:
                        itemResetCountById.Increment(itemInRoomReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInRoomReset.ItemId);
                        break;
                    case ItemInItemReset itemInItemReset:
                        itemResetCountById.Increment(itemInItemReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInItemReset.ItemId);
                        break;
                    case ItemInCharacterReset itemInCharacterReset:
                        itemResetCountById.Increment(itemInCharacterReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInCharacterReset.ItemId);
                        break;
                    case ItemInEquipmentReset itemInEquipmentReset:
                        itemResetCountById.Increment(itemInEquipmentReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInEquipmentReset.ItemId);
                        break;
                }
            }

            // Check local limit + wear location
            foreach (var reset in room.Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset:
                    {
                        int localCount = characterResetCountById[characterReset.CharacterId];
                        int localLimit = characterReset.LocalLimit;
                        if (localCount > localLimit)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Room {0}: M: character {1} found {2} times in room but local limit is {3} -> modifying local limit to {4}", room.Blueprint.Id, characterReset.CharacterId, localCount, localLimit, localCount);
                            characterReset.LocalLimit = localCount;
                        }

                        break;
                    }

                    case ItemInRoomReset itemInRoomReset:
                    {
                        int localCount = itemResetCountById[itemInRoomReset.ItemId];
                        int localLimit = itemInRoomReset.LocalLimit;
                        if (localCount > localLimit)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Room {0}: O: item {1} found {2} times in room but local limit is {3} -> modifying local limit to {4}", room.Blueprint.Id, itemInRoomReset.ItemId, localCount, localLimit, localCount);
                            itemInRoomReset.LocalLimit = localCount;
                        }

                        break;
                    }

                    case ItemInItemReset itemInItemReset:
                    {
                        int localCount = itemResetCountById[itemInItemReset.ItemId];
                        int localLimit = itemInItemReset.LocalLimit;
                        if (localCount > localLimit)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Room {0}: O: item {1} found {2} times in room but local limit is {3} -> modifying local limit to {4}", room.Blueprint.Id, itemInItemReset.ItemId, localCount, localLimit, localCount);
                            itemInItemReset.LocalLimit = localCount;
                        }

                        break;
                    }

                    case ItemInCharacterReset _: // no local limit check
                        break;
                    case ItemInEquipmentReset itemInEquipmentReset: // no local limit check but wear local check
                    {
                        // check wear location
                        var blueprint = ItemManager.GetItemBlueprint(itemInEquipmentReset.ItemId);
                        if (blueprint != null)
                        {
                            if (blueprint.WearLocation == WearLocations.None)
                            {
                                var wearLocations = itemInEquipmentReset.EquipmentSlot.ToWearLocations().ToArray();
                                var newWearLocation = wearLocations.FirstOrDefault(); // TODO: which one to choose from ?
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: E: item {1} has no wear location but reset equipment slot {2} -> modifying item wear location to {3}", room.Blueprint.Id, itemInEquipmentReset.ItemId, itemInEquipmentReset.EquipmentSlot, newWearLocation);
                                blueprint.WearLocation = newWearLocation;
                            }
                            else
                            {
                                var equipmentSlots = blueprint.WearLocation.ToEquipmentSlots().ToArray();
                                if (equipmentSlots.All(x => x != itemInEquipmentReset.EquipmentSlot))
                                {
                                    var newEquipmentSlot = equipmentSlots.First();
                                    Log.Default.WriteLine(LogLevels.Error, "Room {0}: E: item {1} reset equipment slot {2} incompatible with wear location {3} -> modifying reset equipment slot to {4}", room.Blueprint.Id, itemInEquipmentReset.ItemId, itemInEquipmentReset.EquipmentSlot, blueprint.WearLocation, newEquipmentSlot);
                                    itemInEquipmentReset.EquipmentSlot = newEquipmentSlot;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        // Check global = 0 but found in reset
        foreach (var room in RoomManager.Rooms.Where(x => x.Blueprint.Resets?.Count > 0).OrderBy(x => x.Blueprint.Id))
        {
            foreach (var reset in room.Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset:
                    {
                        int globalCount = characterResetGlobalCountById[characterReset.CharacterId];
                        if (characterReset.GlobalLimit == 0)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Room {0}: M: character {1} found {2} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, characterReset.CharacterId, globalCount);
                            characterReset.GlobalLimit = 1;
                        }
                        else if (characterReset.GlobalLimit != -1 && characterReset.GlobalLimit < globalCount)
                            Log.Default.WriteLine(LogLevels.Warning, "Room {0}: M: character {1} found {2} times in world but global limit is {3}", room.Blueprint.Id, characterReset.CharacterId, globalCount, characterReset.GlobalLimit);

                        break;
                    }

                    case ItemInRoomReset _: // no global count check
                        break;
                    case ItemInItemReset _: // no global count check
                        break;
                    case ItemInCharacterReset itemInCharacterReset:
                    {
                        int globalCount = itemResetCountGlobalById[itemInCharacterReset.ItemId];
                        if (itemInCharacterReset.GlobalLimit == 0)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Room {0}: G: item {1} found {2} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, itemInCharacterReset.ItemId, globalCount);
                            itemInCharacterReset.GlobalLimit = 1;
                        }
                        else if (itemInCharacterReset.GlobalLimit != -1 && itemInCharacterReset.GlobalLimit < globalCount)
                            Log.Default.WriteLine(LogLevels.Warning, "Room {0}: G: item {1} found {2} times in world but global limit is {3}", room.Blueprint.Id, itemInCharacterReset.ItemId, globalCount, itemInCharacterReset.GlobalLimit);

                        break;
                    }
                    case ItemInEquipmentReset itemInEquipmentReset:
                    {
                        int globalCount = itemResetCountGlobalById[itemInEquipmentReset.ItemId];
                        if (itemInEquipmentReset.GlobalLimit == 0)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Room {0}: E: item {1} found {2} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, itemInEquipmentReset.ItemId, globalCount);
                            itemInEquipmentReset.GlobalLimit = 1;
                        }
                        else if (itemInEquipmentReset.GlobalLimit != -1 && itemInEquipmentReset.GlobalLimit < globalCount)
                            Log.Default.WriteLine(LogLevels.Warning, "Room {0}: E: item {1} found {2} times in world but global limit is {3}", room.Blueprint.Id, itemInEquipmentReset.ItemId, globalCount, itemInEquipmentReset.GlobalLimit);

                        break;
                    }
                }
            }
        }

        Log.Default.WriteLine(LogLevels.Info, "Resets fixed");
    }
}
