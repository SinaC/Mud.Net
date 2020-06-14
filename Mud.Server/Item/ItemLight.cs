using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemLight : ItemEquipableBase<ItemLightBlueprint>, IItemLight
    {
        private const int Infinite = -1;

        public ItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            DecayPulseLeft = blueprint.DurationHours == Infinite
                ? 0 // infinite
                : blueprint.DurationHours * Settings.PulsePerMinutes*60;
        }

        public ItemLight(Guid guid, ItemLightBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            // Don't overwrite DecayPulseLeft
        }

        // No additional datas
    }
}
