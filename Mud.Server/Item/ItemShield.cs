using System;
using Mud.Server.Blueprints;
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

        #region IItemShield

        public int Armor { get; }

        #endregion
    }
}
