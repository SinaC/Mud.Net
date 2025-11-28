using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Server;

[Export(typeof(IResetManager)), Shared]
public class ResetManager : IResetManager
{
    private ILogger<ResetManager> Logger { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IRandomManager RandomManager { get; }
    private IWiznet Wiznet { get; }

    public ResetManager(ILogger<ResetManager> logger, ICharacterManager characterManager, IItemManager itemManager, IRandomManager randomManager, IWiznet wiznet)
    {
        Logger = logger;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        RandomManager = randomManager;
        Wiznet = wiznet;
    }

    public void ResetArea(IArea area)
    {
        Logger.LogDebug("Resetting {areaName}", area.DisplayName);
        foreach (IRoom room in area.Rooms)
            ResetRoom(room);
        Wiznet.Log($"{area.DisplayName} has just been reset.", WiznetFlags.Resets);
    }

    public void ResetRoom(IRoom room)
    {
        INonPlayableCharacter? lastCharacter = null;
        bool wasPreviousLoaded = false;
        foreach (ResetBase reset in room.Blueprint.Resets)
        {
            switch (reset)
            {
                case CharacterReset characterReset: // 'M'
                    {
                        var blueprint = CharacterManager.GetCharacterBlueprint(characterReset.CharacterId);
                        if (blueprint != null)
                        {
                            int globalCount = characterReset.LocalLimit == -1 ? int.MinValue : CharacterManager.NonPlayableCharacters.Count(x => x.Blueprint.Id == characterReset.CharacterId);
                            if (globalCount < characterReset.GlobalLimit)
                            {
                                int localCount = characterReset.LocalLimit == -1 ? int.MinValue : room.NonPlayableCharacters.Count(x => x.Blueprint.Id == characterReset.CharacterId);
                                if (localCount < characterReset.LocalLimit)
                                {
                                    lastCharacter = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, room);
                                    Logger.LogDebug("Room {blueprintId}: M: Mob {characterId} added", room.Blueprint.Id, characterReset.CharacterId);
                                    wasPreviousLoaded = true;
                                }
                                else
                                    wasPreviousLoaded = false;
                            }
                            else
                                wasPreviousLoaded = false;
                        }
                        else
                            Logger.LogWarning("Room {blueprintId}: M: Mob {characterId} not found", room.Blueprint.Id, characterReset.CharacterId);

                        break;
                    }

                case ItemInRoomReset itemInRoomReset: // 'O'
                    {
                        var blueprint = ItemManager.GetItemBlueprint(itemInRoomReset.ItemId);
                        if (blueprint != null)
                        {
                            // Global limit is not used in stock rom2.4 but used once OLC is added
                            int globalCount = itemInRoomReset.GlobalLimit == -1 ? int.MinValue : ItemManager.Items.Count(x => x.Blueprint.Id == itemInRoomReset.ItemId);
                            if (globalCount < itemInRoomReset.GlobalLimit)
                            {
                                int localCount = itemInRoomReset.LocalLimit == -1 ? int.MinValue : room.Content.Count(x => x.Blueprint.Id == itemInRoomReset.ItemId);
                                if (localCount < itemInRoomReset.LocalLimit)
                                {
                                    var item = ItemManager.AddItem(Guid.NewGuid(), blueprint.Id, room);
                                    Logger.LogDebug("Room {blueprintId}: O: Obj {itemId} added room", room.Blueprint.Id, itemInRoomReset.ItemId);
                                    wasPreviousLoaded = true;
                                }
                                else
                                    wasPreviousLoaded = false;
                            }
                        }
                        else
                            Logger.LogWarning("Room {blueprintId}: O: Obj {itemId} not found", room.Blueprint.Id, itemInRoomReset.ItemId);

                        break;
                    }

                case ItemInItemReset itemInItemReset: // 'P'
                    {
                        var blueprint = ItemManager.GetItemBlueprint(itemInItemReset.ItemId);
                        if (blueprint != null)
                        {
                            // Global limit is not used in stock rom2.4 but used once OLC is added
                            int globalCount = itemInItemReset.GlobalLimit == -1 ? int.MinValue : ItemManager.Items.Count(x => x.Blueprint.Id == itemInItemReset.ItemId);
                            if (globalCount < itemInItemReset.GlobalLimit)
                            {
                                var containerBlueprint = ItemManager.GetItemBlueprint(itemInItemReset.ContainerId);
                                if (containerBlueprint != null)
                                {
                                    if (containerBlueprint is ItemContainerBlueprint || containerBlueprint is ItemCorpseBlueprint)
                                    {
                                        var container = room.Content.OfType<IItemCanContain>().LastOrDefault(x => x.Blueprint.Id == containerBlueprint.Id); // search container in room in stock rom 2.4 it was search in the world)
                                                                                                                                                       // if not found on ground, search in mobile inventory
                                        container ??= room.NonPlayableCharacters.SelectMany(x => x.Inventory.OfType<IItemCanContain>()).LastOrDefault(x => x.Blueprint.Id == containerBlueprint.Id);
                                        // if not found in mobile inventory, search in mobile equipment
                                        container ??= room.NonPlayableCharacters.SelectMany(x => x.Equipments.Where(e => e.Item != null).Select(e => e.Item).OfType<IItemCanContain>()).LastOrDefault(x => x.Blueprint.Id == containerBlueprint.Id);
                                        if (container != null)
                                        {
                                            int localLimit = itemInItemReset.LocalLimit == -1 ? int.MinValue : container.Content.Count(x => x.Blueprint.Id == itemInItemReset.ItemId);
                                            if (localLimit < itemInItemReset.LocalLimit)
                                            {
                                                ItemManager.AddItem(Guid.NewGuid(), blueprint.Id, container);
                                                Logger.LogDebug("Room {blueprintId}: P: Obj {itemId} added in {containerId}", room.Blueprint.Id, itemInItemReset.ItemId, container.Blueprint.Id);
                                                wasPreviousLoaded = true;
                                            }
                                            else
                                                wasPreviousLoaded = false;
                                        }
                                        else
                                        {
                                            Logger.LogWarning("Room {blueprintId}: P: Container Obj {containerId} not found in room nor character in room", room.Blueprint.Id, itemInItemReset.ContainerId);
                                            wasPreviousLoaded = false;
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogWarning("Room {blueprintId}: P: Container Obj {containerId} is not a container", room.Blueprint.Id, itemInItemReset.ContainerId);
                                        wasPreviousLoaded = false;
                                    }
                                }
                                else
                                {
                                    Logger.LogWarning("Room {blueprintId}: P: Container Obj {containerId} not found", room.Blueprint.Id, itemInItemReset.ContainerId);
                                    wasPreviousLoaded = false;
                                }
                            }
                        }
                        else
                            Logger.LogWarning("Room {blueprintId}: P: Obj {itemId} not found", room.Blueprint.Id, itemInItemReset.ItemId);

                        break;
                    }

                case ItemInCharacterReset itemInCharacterReset: // 'G'
                    {
                        var blueprint = ItemManager.GetItemBlueprint(itemInCharacterReset.ItemId);
                        if (blueprint != null)
                        {
                            if (wasPreviousLoaded)
                            {
                                int globalCount = itemInCharacterReset.GlobalLimit == -1 ? int.MinValue : ItemManager.Items.Count(x => x.Blueprint.Id == itemInCharacterReset.ItemId);
                                if (globalCount < itemInCharacterReset.GlobalLimit)
                                {
                                    if (lastCharacter != null)
                                    {
                                        var item = ItemManager.AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        if (item != null)
                                        {
                                            if (lastCharacter.Blueprint is CharacterShopBlueprint)
                                            {
                                                // TODO: randomize level
                                                item.AddBaseItemFlags(false, "Inventory");
                                                item.Recompute();
                                            }
                                            Logger.LogDebug("Room {blueprintId}: G: Obj {itemId} added on {lastCharacterId}", room.Blueprint.Id, itemInCharacterReset.ItemId, lastCharacter.Blueprint.Id);
                                            wasPreviousLoaded = true;
                                        }
                                        else
                                        {
                                            Logger.LogError("Room {blueprintId}: G: Obj {itemId} NOT added on {lastCharacterId}", room.Blueprint.Id, itemInCharacterReset.ItemId, lastCharacter.Blueprint.Id);
                                            wasPreviousLoaded = false;
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogWarning("Room {blueprintId}: G: Obj {itemId} No last character", room.Blueprint.Id, itemInCharacterReset.ItemId);
                                        wasPreviousLoaded = false;
                                    }
                                }
                            }
                            else
                                Logger.LogWarning("Room {blueprintId}: G: Obj {itemId} previous reset was not loaded successfully", room.Blueprint.Id, itemInCharacterReset.ItemId);
                        }
                        else
                            Logger.LogWarning("Room {blueprintId}: G: Obj {itemId} not found", room.Blueprint.Id, itemInCharacterReset.ItemId);

                        break;
                    }

                case ItemInEquipmentReset itemInEquipmentReset: // 'E'
                    {
                        var blueprint = ItemManager.GetItemBlueprint(itemInEquipmentReset.ItemId);
                        if (blueprint != null)
                        {
                            if (wasPreviousLoaded)
                            {
                                int globalCount = itemInEquipmentReset.GlobalLimit == -1 ? int.MinValue : ItemManager.Items.Count(x => x.Blueprint.Id == itemInEquipmentReset.ItemId);
                                if (globalCount < itemInEquipmentReset.GlobalLimit)
                                {
                                    if (lastCharacter != null)
                                    {
                                        var item = ItemManager.AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        if (item != null)
                                        {
                                            Logger.LogDebug("Room {blueprintId}: E: Obj {itemId} added on {lastCharacterId}", room.Blueprint.Id, itemInEquipmentReset.ItemId, lastCharacter.Blueprint.Id);
                                            wasPreviousLoaded = true;
                                            // try to equip
                                            if (item.WearLocation != WearLocations.None)
                                            {
                                                var equippedItem = lastCharacter.SearchEquipmentSlot(item, false);
                                                if (equippedItem != null)
                                                {
                                                    equippedItem.Item = item;
                                                    item.ChangeContainer(null); // remove from inventory
                                                    item.ChangeEquippedBy(lastCharacter, true); // set as equipped by lastCharacter
                                                }
                                                else
                                                    Logger.LogWarning("Room {blueprintId}: E: Obj {itemId} wear location {wearLocation} doesn't exist on last character {lastCharacterId}", room.Blueprint.Id, itemInEquipmentReset.ItemId, item.WearLocation, lastCharacter.Blueprint.Id);
                                            }
                                            else
                                                Logger.LogWarning("Room {blueprintId}: E: Obj {itemId} cannot be equipped", room.Blueprint.Id, itemInEquipmentReset.ItemId);
                                        }
                                        else
                                        {
                                            Logger.LogError("Room {blueprintId}: E: Obj {itemId} NOT added on {lastCharacterId}", room.Blueprint.Id, itemInEquipmentReset.ItemId, lastCharacter.Blueprint.Id);
                                            wasPreviousLoaded = false;

                                        }
                                    }
                                    else
                                    {
                                        Logger.LogWarning("Room {blueprintId}: E: Obj {itemId} Last character doesn't exist", room.Blueprint.Id, itemInEquipmentReset.ItemId);
                                        wasPreviousLoaded = false;
                                    }
                                }
                            }
                            else
                                Logger.LogWarning("Room {blueprintId}: E: Obj {itemId} previous reset was not loaded successfully", room.Blueprint.Id, itemInEquipmentReset.ItemId);
                        }
                        else
                            Logger.LogWarning("Room {blueprintId}: E: Obj {itemId} not found", room.Blueprint.Id, itemInEquipmentReset.ItemId);

                        break;
                    }

                case DoorReset doorReset: // 'D'
                    {
                        var exit = room.Exits[(int)doorReset.ExitDirection];
                        if (exit != null)
                        {
                            switch (doorReset.Operation)
                            {
                                case DoorOperations.OpenedAndUnlocked:
                                    Logger.LogDebug("Room {blueprintId}: D: set opened/unlocked {exitDirection}", room.Blueprint.Id, doorReset.ExitDirection);
                                    exit.Open();
                                    exit.Unlock();
                                    break;
                                case DoorOperations.ClosedAndUnlocked:
                                    Logger.LogDebug("Room {blueprintId}: D: set closed/unlocked {exitDirection}", room.Blueprint.Id, doorReset.ExitDirection);
                                    exit.Close();
                                    exit.Unlock();
                                    break;
                                case DoorOperations.ClosedAndLocked:
                                    Logger.LogDebug("Room {blueprintId}: D: set closed/locked {exitDirection}", room.Blueprint.Id, doorReset.ExitDirection);
                                    exit.Close();
                                    exit.Lock();
                                    break;
                                default:
                                    Logger.LogWarning("Room {blueprintId}: D: Invalid operation {operation} for exit {exitDirection}", room.Blueprint.Id, doorReset.Operation, doorReset.ExitDirection);
                                    break;
                            }
                        }
                        else
                            Logger.LogWarning("Room {blueprintId}: D: Invalid exit {exitDirection}", room.Blueprint.Id, doorReset.ExitDirection);

                        break;
                    }
                case RandomizeExitsReset randomizeExitsReset: // 'R'
                    {
                        for (var idx = 0; idx < randomizeExitsReset.MaxDirections; idx++)
                        {
                            // get random direction within range
                            var randomIdx = RandomManager.Next(randomizeExitsReset.MaxDirections);
                            // swap exits
                            var exit = room.Exits[idx];
                            room.Exits[idx] = room.Exits[randomIdx];
                            room.Exits[randomIdx] = exit;
                        }
                        break;
                    }
            }
        }
    }
}
