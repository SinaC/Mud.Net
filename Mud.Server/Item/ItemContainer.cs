using System;
using System.Collections.Generic;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemBase, IContainer
    {
        private readonly List<IItem> _content;

        public ItemContainer(Guid guid, string name, IContainer containedInto)
            : base(guid, name, containedInto)
        {
            _content = new List<IItem>();
        }

        #region IContainer

        public IReadOnlyCollection<IItem> Content
        {
            get { return _content.AsReadOnly(); }
        }

        public bool Put(IItem obj)
        {
            // TODO: check if already in a container
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
