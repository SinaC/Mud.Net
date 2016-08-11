using System;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Blueprints;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Item
{
    public abstract class ItemBase<TBlueprint> : EntityBase, IItem
        where TBlueprint : ItemBlueprintBase
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> ItemCommands;

        static ItemBase()
        {
            ItemCommands = CommandHelpers.GetCommands(typeof (ItemBase<TBlueprint>));
        }

        protected ItemBase(Guid guid, TBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            ContainedInto = containedInto;
            containedInto.PutInContainer(this);
            Weight = blueprint.Weight;
            Cost = blueprint.Cost;
            IsWearable = true; // TODO
        }

        #region IItem

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => ItemCommands;

        #endregion

        public override string DisplayName => Blueprint == null ? StringHelpers.UpperFirstLetter(Name) : Blueprint.ShortDescription;

        public override void OnRemoved() // called before removing an item from the game
        {
            base.OnRemoved();
            ContainedInto?.GetFromContainer(this);
            ContainedInto = null;
            Blueprint = null;
        }

        #endregion

        public IContainer ContainedInto { get; private set; }

        public ItemBlueprintBase Blueprint { get; private set; }

        public bool IsWearable { get; private set; } // TODO:

		public int DecayPulseLeft { get; protected set; } // 0: means no decay

        public virtual int Weight { get; }

        public virtual int Cost { get; }

        public bool ChangeContainer(IContainer container)
        {
            Log.Default.WriteLine(LogLevels.Info, "ChangeContainer: {0} : {1} -> {2}", DisplayName, ContainedInto == null ? "<<??>>" : ContainedInto.DisplayName, container == null ? "<<??>>" : container.DisplayName);

            ContainedInto?.GetFromContainer(this);
            //Debug.Assert(container != null, "ChangeContainer: an item cannot be outside a container"); // False, equipment are not stored in any container
            //container.PutInContainer(this);
            //ContainedInto = container;
            container?.PutInContainer(this);
            ContainedInto = container;
            return true;
        }

        public void DecreaseDecayPulseLeft()
        {
            if (DecayPulseLeft > 0)
                DecayPulseLeft--;
        }

        #endregion
    }
}
