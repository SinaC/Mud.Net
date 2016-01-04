using System;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemArmor : ItemBase, IItemArmor
    {
        public ItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = blueprint.WearLocation;
            Armor = blueprint.Armor;
            ArmorKind = blueprint.ArmorKind;
        }

        public WearLocations WearLocation { get; private set; }
        public int Armor { get; private set; }
        public ArmorKinds ArmorKind { get; private set; }
    }
}
