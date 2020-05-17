using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemLight : ItemBase<ItemLightBlueprint, ItemData>, IItemLight
    {
        private const int Infinite = -1;

        public ItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            TimeLeft = blueprint.DurationHours == Infinite
                ? Infinite
                : blueprint.DurationHours * 60;
        }

        public ItemLight(Guid guid, ItemLightBlueprint blueprint, ItemLightData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            TimeLeft = itemData.TimeLeft;
        }

        #region IItemLight

        public bool IsLighten => TimeLeft == Infinite || TimeLeft > 0;

        public int TimeLeft { get; protected set; }

        public bool IsInfinite => TimeLeft == Infinite;

        public bool DecreaseTimeLeft()
        {
            if (TimeLeft != Infinite && TimeLeft > 0)
                TimeLeft--;
            return TimeLeft == 0;
        }

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemLightData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                Auras = MapAuraData(),
                TimeLeft = TimeLeft,
            };
        }

        #endregion
    }
}
