using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemBase<ItemContainerBlueprint>, IItemContainer
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

        #region IItem

        public override int Weight
        {
            get { return base.Weight + _content.Sum(x => x.Weight)*WeightMultiplier; }
        }

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
    }
}
