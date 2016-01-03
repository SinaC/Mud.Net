using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemArmor : ItemBase
    {
        public WearLocations WearLocation { get; private set; }

        public int Pierce { get; private set; }
        public int Bash { get; private set; }
        public int Slash { get; private set; }
        public int Exotic { get; private set; }

        public ItemArmor(Guid guid, ItemBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = (WearLocations)blueprint.WearLocation;

            Pierce = blueprint.Values[0]; // TODO: sub blueprint
            Bash = blueprint.Values[1];
            Slash = blueprint.Values[2];
            Exotic = blueprint.Values[3];
        }
    }
}
