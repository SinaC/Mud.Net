using Mud.Server.Constants;

namespace Mud.Server.Blueprints.Item
{
    public class ItemQuestBlueprint : ItemBlueprintBase
    {
        public ItemQuestBlueprint()
        {
            WearLocation = WearLocations.None;
            Weight = 0;
            Cost = 0;
            // TODO: Cannot be dropped (but can be destroyed)
        }
    }
}
