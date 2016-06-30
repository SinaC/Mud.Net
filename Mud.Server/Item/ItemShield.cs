using System;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemShield : ItemEquipableBase, IItemShield
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
