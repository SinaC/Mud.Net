using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Server
{
    public class ResetManager : IResetManager
    {
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }
        private IWiznet Wiznet { get; }

        public ResetManager(ICharacterManager characterManager, IItemManager itemManager, IWiznet wiznet)
        {
            CharacterManager = characterManager;
            ItemManager = itemManager;
            Wiznet = wiznet;
        }

        public void ResetArea(IArea area)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Resetting {0}", area.DisplayName);
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
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: M: Mob {characterReset.CharacterId} added");
                                        wasPreviousLoaded = true;
                                    }
                                    else
                                        wasPreviousLoaded = false;
                                }
                                else
                                    wasPreviousLoaded = false;
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: M: Mob {characterReset.CharacterId} not found");

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
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: O: Obj {itemInRoomReset.ItemId} added room");
                                        wasPreviousLoaded = true;
                                    }
                                    else
                                        wasPreviousLoaded = false;
                                }
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: O: Obj {itemInRoomReset.ItemId} not found");

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
                                                    Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: P: Obj {itemInItemReset.ItemId} added in {container.Blueprint.Id}");
                                                    wasPreviousLoaded = true;
                                                }
                                                else
                                                    wasPreviousLoaded = false;
                                            }
                                            else
                                            {
                                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: P: Container Obj {itemInItemReset.ContainerId} not found in room nor character in room");
                                                wasPreviousLoaded = false;
                                            }
                                        }
                                        else
                                        {
                                            Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: P: Container Obj {itemInItemReset.ContainerId} is not a container");
                                            wasPreviousLoaded = false;
                                        }
                                    }
                                    else
                                    {
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: P: Container Obj {itemInItemReset.ContainerId} not found");
                                        wasPreviousLoaded = false;
                                    }
                                }
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: P: Obj {itemInItemReset.ItemId} not found");

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
                                                Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} added on {lastCharacter.Blueprint.Id}");
                                                wasPreviousLoaded = true;
                                            }
                                            else
                                            {
                                                Log.Default.WriteLine(LogLevels.Error, $"Room {room.Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} NOT added on {lastCharacter.Blueprint.Id}");
                                                wasPreviousLoaded = false;
                                            }
                                        }
                                        else
                                        {
                                            Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} No last character");
                                            wasPreviousLoaded = false;
                                        }
                                    }
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} previous reset was not loaded successfully");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} not found");

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
                                                Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} added on {lastCharacter.Blueprint.Id}");
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
                                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} wear location {item.WearLocation} doesn't exist on last character {lastCharacter.Blueprint.Id}");
                                                }
                                                else
                                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} cannot be equipped");
                                            }
                                            else
                                            {
                                                Log.Default.WriteLine(LogLevels.Error, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} NOT added on {lastCharacter.Blueprint.Id}");
                                                wasPreviousLoaded = false;

                                            }
                                        }
                                        else
                                        {
                                            Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} Last character doesn't exist");
                                            wasPreviousLoaded = false;
                                        }
                                    }
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} previous reset was not loaded successfully");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} not found");

                            break;
                        }

                    case DoorReset doorReset: // 'D'
                        {
                            var exit = room.Exits[(int)doorReset.ExitDirection];
                            if (exit != null)
                            {
                                switch (doorReset.Operation)
                                {
                                    case 0:
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: D: Open/Unlock {doorReset.ExitDirection}");
                                        exit.Open();
                                        exit.Unlock();
                                        break;
                                    case 1:
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: D: Close/Unlock {doorReset.ExitDirection}");
                                        exit.Close();
                                        exit.Unlock();
                                        break;
                                    case 2:
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {room.Blueprint.Id}: D: Close/Lock {doorReset.ExitDirection}");
                                        exit.Close();
                                        exit.Lock();
                                        break;
                                    default:
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: D: Invalid operation {doorReset.Operation} for exit {doorReset.ExitDirection}");
                                        break;
                                }
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {room.Blueprint.Id}: D: Invalid exit {doorReset.ExitDirection}");

                            break;
                        }
                        // TODO: R: randomize room exits
                }
            }
        }
    }
}
