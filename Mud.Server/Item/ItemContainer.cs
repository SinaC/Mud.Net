using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemBase, IContainer
    {
        private readonly List<IItem> _content;

        public int ItemCount { get; private set; } // maximum number of items
        public int WeightMultiplier { get; private set; } // percentage

        public ItemContainer(Guid guid, ItemBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            _content = new List<IItem>();
            // TODO: key, flags
            ItemCount = blueprint.Values[3];
            WeightMultiplier = blueprint.Values[4];
        }

        #region IItem

        public override int Weight
        {
            get
            {
                return base.Weight + _content.Sum(x => x.Weight)*WeightMultiplier;
            }
        }

        #endregion

        #region IContainer

        public IReadOnlyCollection<IItem> Content
        {
            get { return _content.AsReadOnly(); }
        }

        public bool Put(IItem obj)
        {
            // TODO: check if already in a container, check max items
            _content.Add(obj);
            return true;
        }

        public bool Get(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion
    }
}
