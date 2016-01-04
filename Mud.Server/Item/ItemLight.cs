using System;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemLight : ItemBase, IItemLight
    {
        public ItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            TimeLeft = blueprint.DurationHours;
        }

        #region IItemLight

        // -1: infinite
        public int TimeLeft { get; private set; }

        public void Consume()
        {
            if (TimeLeft >= 0)
                TimeLeft--;
        }
        #endregion
    }
}
