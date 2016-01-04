using System;
using System.Diagnostics;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Item
{
    public abstract class ItemBase : EntityBase, IItem
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> ItemCommands;

        private int _weight;
        private int _cost;

        static ItemBase()
        {
            ItemCommands = CommandHelpers.GetCommands(typeof (ItemBase));
        }

        protected ItemBase(Guid guid, ItemBlueprintBase blueprint, IContainer containedInto)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            ContainedInto = containedInto;
            containedInto.Put(this);
            _weight = blueprint.Weight;
            IsWearable = true; // TODO
        }

        #region IItem

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return ItemCommands; }
        }

        #endregion

        public override string DisplayName
        {
            get { return Blueprint == null ? StringHelpers.UpperFirstLetter(Name) : Blueprint.ShortDescription; }
        }

        #endregion

        public IContainer ContainedInto { get; private set; }

        public ItemBlueprintBase Blueprint { get; private set; }

        public bool IsWearable { get; private set; } // TODO:

        public virtual int Weight
        {
            get { return _weight; }
        }

        public virtual int Cost
        {
            get { return _cost; }
        }

        public bool ChangeContainer(IContainer container)
        {
            // TODO: check if can be put in a container
            if (ContainedInto != null)
                ContainedInto.Get(this);
            Debug.Assert(container != null, "ChangeContainer: an item cannot be outside a container");
            //if (container != null) // Cannot be null
            container.Put(this);
            ContainedInto = container;
            return true;
        }

        #endregion
    }
}
