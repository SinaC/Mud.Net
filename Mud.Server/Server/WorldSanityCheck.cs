using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.Reset;
using Mud.Blueprints.Room;
using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Server
{
    [Export(typeof(ISanityCheck)), Shared]
    public class WorldSanityCheck : ISanityCheck
    {
        private ILogger<WorldSanityCheck> Logger { get; }
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }
        private WorldOptions WorldOptions { get; }

        public WorldSanityCheck(ILogger<WorldSanityCheck> logger, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IOptions<WorldOptions> options)
        {
            Logger = logger;
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
            WorldOptions = options.Value;
        }

        public bool PerformSanityChecks()
        {
            var fatalErrorFound = false;

            fatalErrorFound |= CheckMandatoryRooms();
            fatalErrorFound |= CheckMandatoryItems();
            fatalErrorFound |= CheckPetShops();
            fatalErrorFound |= CheckExitWithInvalidKeys();

            return fatalErrorFound;
        }

        private bool CheckMandatoryRooms()
        {
            var fatalErrorFound = false;
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.NullRoom) == null)
            {
                Logger.LogCritical("Room NullRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.NullRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.DefaultRoom) == null)
            {
                Logger.LogCritical("Room DefaultRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.DefaultRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.DefaultRecallRoom) == null)
            {
                Logger.LogCritical("Room DefaultRecallRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.DefaultRecallRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.DefaultDeathRoom) == null)
            {
                Logger.LogCritical("Room DefaultDeathRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.DefaultDeathRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.MudSchoolRoom) == null)
            {
                Logger.LogCritical("Room MudSchoolRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.MudSchoolRoom);
                fatalErrorFound = true;
            }
            return fatalErrorFound;
        }

        private bool CheckMandatoryItems()
        {
            var fatalErrorFound = false;
            if (ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(WorldOptions.BlueprintIds.Corpse) == null)
            {
                Logger.LogCritical("Item Corpse blueprint {blueprintId} not found or not a corpse", WorldOptions.BlueprintIds.Corpse);
                fatalErrorFound = true;
            }
            if (ItemManager.GetItemBlueprint<ItemMoneyBlueprint>(WorldOptions.BlueprintIds.Coins) == null)
            {
                Logger.LogCritical("Item Coins blueprint {blueprintId} not found or not money", WorldOptions.BlueprintIds.Coins);
                fatalErrorFound = true;
            }
            return fatalErrorFound;
        }

        private bool CheckPetShops()
        {
            // pet shops
            foreach (var petShopBlueprint in CharacterManager.CharacterBlueprints.OfType<CharacterPetShopBlueprint>().OrderBy(x => x.Id))
            {
                foreach (var petBlueprintId in petShopBlueprint.PetBlueprintIds.Distinct())
                {
                    var petBlueprint = CharacterManager.GetCharacterBlueprint(petBlueprintId);
                    if (petBlueprint == null)
                        Logger.LogError("Pet {petBlueprintId} sold by pet shop keeper {petShopBlueprintId} not found", petBlueprintId, petShopBlueprint.Id);
                }
            }
            return false; // invalid shop is not a fatal error
        }

        private bool CheckExitWithInvalidKeys()
        {
            var items = new List<(RoomBlueprint roomBlueprint, ExitBlueprint exitBlueprint, ItemKeyBlueprint itemKeyBlueprint)>();
            foreach (var roomBlueprint in RoomManager.RoomBlueprints.OrderBy(x => x.Id))
            {
                foreach (var exitBlueprint in roomBlueprint.Exits.Where(x => x is not null))
                {
                    if (exitBlueprint.Key > 0)
                    {
                        var keyBlueprint = ItemManager.GetItemBlueprint(exitBlueprint.Key);
                        if (keyBlueprint == null)
                            Logger.LogError("Room {blueprintId} exit {exit} key {key} doesn't exist", roomBlueprint.Id, exitBlueprint.Direction, exitBlueprint.Key);
                        else if (keyBlueprint is not ItemKeyBlueprint itemKeyBlueprint)
                            Logger.LogError("Room {blueprintId} exit {exit} key {key} is not a key but {blueprint}", roomBlueprint.Id, exitBlueprint.Direction, exitBlueprint.Key, keyBlueprint.GetType().FullName);
                        else
                        {
                            items.Add((roomBlueprint, exitBlueprint, itemKeyBlueprint));
                        }
                    }
                }
            }

            // search keys in resets
            foreach (var groupedByKey in items.GroupBy(x => x.itemKeyBlueprint.Id).OrderBy(x => x.Key))
            {
                var found = false;
                foreach (var roomBlueprint in RoomManager.RoomBlueprints)
                {
                    found = roomBlueprint.Resets.OfType<ItemInCharacterReset>().Any(x => x.ItemId == groupedByKey.Key);
                    if (found)
                        break;
                    found = roomBlueprint.Resets.OfType<ItemInItemReset>().Any(x => x.ItemId == groupedByKey.Key);
                    if (found)
                        break;
                    found = roomBlueprint.Resets.OfType<ItemInRoomReset>().Any(x => x.ItemId == groupedByKey.Key);
                    if (found)
                        break;
                    found = roomBlueprint.Resets.OfType<ItemInEquipmentReset>().Any(x => x.ItemId == groupedByKey.Key);
                    if (found)
                        break;
                }
                if (!found)
                    Logger.LogError("Key {key} needed to open {roomAndExits} is not found anywhere", groupedByKey.Key, string.Join(",", groupedByKey.Select(x => $"(room:{x.roomBlueprint.Id} [exit:{x.exitBlueprint.Direction}])")));
            }

            return false; // invalid room exit is not a fatal error
        }
    }
}
