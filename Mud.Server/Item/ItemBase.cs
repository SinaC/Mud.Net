using System;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Input;

namespace Mud.Server.Item
{
    public abstract class ItemBase : EntityBase, IItem
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> ItemCommands;

        static ItemBase()
        {
            ItemCommands = CommandHelpers.GetCommands(typeof (ItemBase));
        }

        protected ItemBase(Guid guid, string name, IContainer containedInto)
            : base(guid, name)
        {
            ContainedInto = containedInto;
            containedInto.Put(this);
        }

        #region IItem

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return ItemCommands; }
        }

        #endregion

        public IContainer ContainedInto { get; private set; }

        public ItemBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public bool ChangeContainer(IContainer container)
        {
            // TODO: check if can be put in a container
            ContainedInto = container;
            return true;
        }

        #endregion
    }
}
