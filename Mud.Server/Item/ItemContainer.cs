using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemBase, IItemContainer
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

        public int ItemCount { get; private set; } // maximum number of items
        public int WeightMultiplier { get; private set; } // percentage

        #region IContainer

        public IReadOnlyCollection<IItem> Content
        {
            get { return _content.AsReadOnly(); }
        }

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container, check max items
            _content.Add(obj);
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
