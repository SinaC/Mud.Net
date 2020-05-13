using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemArmor : ItemBase<ItemArmorBlueprint, ItemData>, IItemArmor
    {
        public ItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Bash = blueprint.Bash;
            Pierce = blueprint.Pierce;
            Slash = blueprint.Slash;
            Exotic = blueprint.Exotic;
        }

        public ItemArmor(Guid guid, ItemArmorBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Bash = blueprint.Bash;
            Pierce = blueprint.Pierce;
            Slash = blueprint.Slash;
            Exotic = blueprint.Exotic;
        }

        #region IItemArmor

        public int Bash { get; }
        public int Pierce { get; }
        public int Slash { get; }
        public int Exotic { get; }

        #endregion
    }
}
