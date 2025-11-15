using Mud.Domain;

namespace Mud.Server.Blueprints.Item;

public class ItemQuestBlueprint : ItemBlueprintBase
{
    public ItemQuestBlueprint()
    {
        WearLocation = WearLocations.None;
        Weight = 0;
        Cost = 0;
    }
}
