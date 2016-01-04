using System;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemArmor : ItemBase
    {
        public WearLocations WearLocation { get; private set; }

        public int Pierce { get; private set; }
        public int Bash { get; private set; }
        public int Slash { get; private set; }
        public int Exotic { get; private set; }

        public ItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = blueprint.WearLocation;

            Pierce = blueprint.Pierce;
            Bash = blueprint.Bash;
            Slash = blueprint.Slash;
            Exotic = blueprint.Exotic;
        }
    }
}
