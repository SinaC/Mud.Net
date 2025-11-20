using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24
{
    public class NeededBlueprintsSanityCheck : ISanityCheck
    {
        private ILogger<NeededBlueprintsSanityCheck> Logger { get; }
        private IItemManager ItemManager { get; }
        private int MushroomBlueprintId { get; }
        private int SpringBlueprintId { get; }
        private int LightBallBlueprintId { get; }
        private int FloatingDiscBlueprintId { get; }
        private int PortalBlueprintId { get; }
        private int RoseBlueprintId { get; }

        public NeededBlueprintsSanityCheck(ILogger<NeededBlueprintsSanityCheck> logger, IOptions<Rom24Options> options, IItemManager itemManager)
        {
            Logger = logger;
            ItemManager = itemManager;
            MushroomBlueprintId = options.Value.BlueprintIds.Mushroom;
            SpringBlueprintId = options.Value.BlueprintIds.Spring;
            LightBallBlueprintId = options.Value.BlueprintIds.LightBall;
            FloatingDiscBlueprintId = options.Value.BlueprintIds.FloatingDisc;
            PortalBlueprintId = options.Value.BlueprintIds.Portal;
            RoseBlueprintId = options.Value.BlueprintIds.Rose;
        }

        public bool PerformSanityChecks()
        {
            if (ItemManager.GetItemBlueprint<ItemFoodBlueprint>(MushroomBlueprintId) == null)
                Logger.LogError("'a Magic mushroom' blueprint {blueprintId} not found or not food (needed for spell CreateFood)", MushroomBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemFountainBlueprint>(SpringBlueprintId) == null)
                Logger.LogError("'a magical spring' blueprint {blueprintId} not found or not a fountain (needed for spell CreateSpring)", SpringBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemLightBlueprint>(LightBallBlueprintId) == null)
                Logger.LogError("'a bright ball of light' blueprint {blueprintId} not found or not a light (needed for spell ContinualLight)", LightBallBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemContainerBlueprint>(FloatingDiscBlueprintId) == null)
                Logger.LogError("'a floating disc' blueprint {blueprintId} not found or not a container (needed for spell FloatingDisc)", FloatingDiscBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemPortalBlueprint>(PortalBlueprintId) == null)
                Logger.LogError("'a portal' blueprint {blueprintId} not found or not a portal (needed for spells Portal and Nexus)", PortalBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemTrashBlueprint>(RoseBlueprintId) == null)
                Logger.LogError("'a beautiful rose' blueprint {blueprintId} not found or not a trash (needed for spell CreateRose)", RoseBlueprintId);
            return true;
        }
    }
}
