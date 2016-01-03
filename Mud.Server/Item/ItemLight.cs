using System;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemLight : ItemBase
    {
        // -1: infinite
        public int TimeLeft { get; private set; }

        public ItemLight(Guid guid, ItemBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            TimeLeft = blueprint.Values[2];
        }

        public void Consume()
        {
            if (TimeLeft >= 0)
                TimeLeft--;
        }
    }
}
