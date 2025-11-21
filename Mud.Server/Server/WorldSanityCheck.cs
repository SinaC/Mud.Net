using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Diagnostics;

namespace Mud.Server.Server
{
    public class WorldSanityCheck : ISanityCheck
    {
        private ILogger<WorldSanityCheck> Logger { get; }
        private IRoomManager RoomManager { get; }
        private IItemManager ItemManager { get; }
        private WorldOptions WorldOptions { get; }

        public WorldSanityCheck(ILogger<WorldSanityCheck> logger, IRoomManager roomManager, IItemManager itemManager, IOptions<WorldOptions> options)
        {
            Logger = logger;
            RoomManager = roomManager;
            ItemManager = itemManager;
            WorldOptions = options.Value;
        }

        public bool PerformSanityChecks()
        {
            bool fatalErrorFound = false;
            // rooms
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.NullRoom) == null)
            {
                Logger.LogError("Room NullRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.NullRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.DefaultRoom) == null)
            {
                Logger.LogError("Room DefaultRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.DefaultRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.DefaultRecallRoom) == null)
            {
                Logger.LogError("Room DefaultRecallRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.DefaultRecallRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.DefaultDeathRoom) == null)
            {
                Logger.LogError("Room DefaultDeathRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.DefaultDeathRoom);
                fatalErrorFound = true;
            }
            if (RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.MudSchoolRoom) == null)
            {
                Logger.LogError("Room MudSchoolRoom blueprint {blueprintId} not found", WorldOptions.BlueprintIds.MudSchoolRoom);
                fatalErrorFound = true;
            }
            // items
            if (ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(WorldOptions.BlueprintIds.Corpse) == null)
            {
                Logger.LogError("Item Corpse blueprint {blueprintId} not found or not a corpse", WorldOptions.BlueprintIds.Corpse);
                fatalErrorFound = true;
            }
            if (ItemManager.GetItemBlueprint<ItemMoneyBlueprint>(WorldOptions.BlueprintIds.Coins) == null)
            {
                Logger.LogError("Item Coins blueprint {blueprintId} not found or not money", WorldOptions.BlueprintIds.Coins);
                fatalErrorFound = true;
            }
            return fatalErrorFound;
        }
    }
}
