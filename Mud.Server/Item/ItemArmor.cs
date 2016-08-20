using System;
using Mud.Server.Blueprints;
using Mud.Server.Blueprints.Item;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemArmor : ItemEquipableBase<ItemArmorBlueprint>, IItemArmor
    {
        public ItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Armor = blueprint.Armor;
            ArmorKind = blueprint.ArmorKind;
        }

        public int Armor { get; }
        public ArmorKinds ArmorKind { get; }
    }
}
