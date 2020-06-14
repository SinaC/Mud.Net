using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

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

        public ItemArmor(Guid guid, ItemArmorBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Armor = blueprint.Armor;
            ArmorKind = blueprint.ArmorKind;
        }

        public int Armor { get; }
        public ArmorKinds ArmorKind { get; }
    }
}
