using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;

namespace Mud.Server.Item
{
    public class ItemLight : ItemEquippableBase<ItemLightBlueprint>, IItemLight
    {
        private const int Infinite = -1;

        public ItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            DecayPulseLeft = blueprint.DurationHours == Infinite
                ? 0 // infinite
                : blueprint.DurationHours * Pulse.PulsePerMinutes*60;
        }

        public ItemLight(Guid guid, ItemLightBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            // Don't overwrite DecayPulseLeft
        }

        #region IItemLight

        public bool IsLighten => DecayPulseLeft == Infinite || DecayPulseLeft > 0;

        #endregion

        // No additional datas
    }
}
