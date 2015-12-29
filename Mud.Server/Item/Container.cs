using System;
using System.Collections.Generic;

namespace Mud.Server.Item
{
    public class Container : ItemBase, IContainer
    {
        private readonly List<IItem> _inside;

        public Container(Guid guid, string name, IContainer containedInto)
            : base(guid, name, containedInto)
        {
            _inside = new List<IItem>();
        }

        #region IContainer

        public IReadOnlyCollection<IItem> Inside
        {
            get { return _inside.AsReadOnly(); }
        }

        public bool Put(IItem obj)
        {
            // TODO: check if already in a container
            _inside.Add(obj);
            return true;
        }

        public bool Get(IItem obj)
        {
            bool removed = _inside.Remove(obj);
            return removed;
        }

        #endregion
    }
}
