using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemShield : ItemEquipableBase<ItemShieldBlueprint>, IItemShield
    {
        public ItemShield(Guid guid, ItemShieldBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Armor = blueprint.Armor;
        }

        public ItemShield(Guid guid, ItemShieldBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Armor = blueprint.Armor;
        }

        #region IItemShield

        public int Armor { get; }

        #endregion
    }
}
