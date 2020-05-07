using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemEquipableBase<ItemContainerBlueprint>, IItemContainer
    {
        private readonly List<IItem> _content;

        public ItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            _content = new List<IItem>();
            // TODO: key, flags
            ItemCount = blueprint.ItemCount;
            WeightMultiplier = blueprint.WeightMultiplier;
        }

        public ItemContainer(Guid guid, ItemContainerBlueprint blueprint, ItemContainerData itemContainerData, IContainer containedInto)
            : base(guid, blueprint, itemContainerData, containedInto)
        {
            _content = new List<IItem>();
            // TODO: key, flags
            ItemCount = blueprint.ItemCount;
            WeightMultiplier = blueprint.WeightMultiplier;

            if (itemContainerData.Contains?.Length > 0)
            {
                foreach (ItemData itemData in itemContainerData.Contains)
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
        }

        #region IItem

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemContainerData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                Contains = MapContent(),
                Auras = MapAuraData(),
            };
        }

        #endregion

        public override int Weight => base.Weight + _content.Sum(x => x.Weight)*WeightMultiplier;

        void IEntity.OnRemoved()
        {
            _content.Clear();
        }

        #endregion

        #region IItemContainer

        public int ItemCount { get; } // maximum number of items
        public int WeightMultiplier { get; } // percentage

        #region IContainer

        public IEnumerable<IItem> Content => _content;

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container, check max items
            _content.Insert(0, obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion

        #endregion

        private ItemData[] MapContent()
        {
            if (Content.Any())
                return Content.Select(x => x.MapItemData()).ToArray();
            return null;
        }
    }
}
