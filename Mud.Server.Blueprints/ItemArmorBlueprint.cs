using Mud.Server.Constants;

namespace Mud.Server.Blueprints
{
    public class ItemArmorBlueprint : ItemBlueprintBase
    {
        public int Armor { get; set; }
        public ArmorKinds ArmorKind { get; set; }
    }
}
