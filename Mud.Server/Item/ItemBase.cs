using System;
using System.Collections.Generic;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Item
{
    public abstract class ItemBase<TBlueprint> : EntityBase, IItem
        where TBlueprint : ItemBlueprintBase
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> ItemCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(ItemBase<TBlueprint>)));

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

        public override IReadOnlyTrie<CommandMethodInfo> Commands => ItemCommands.Value;

        #endregion

        public override string DisplayName => Blueprint == null ? StringHelpers.UpperFirstLetter(Name) : Blueprint.ShortDescription;

        public override string DebugName => Blueprint == null ? DisplayName : $"{DisplayName}[{Blueprint.Id}]";

        public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
        {
            StringBuilder displayName = new StringBuilder();
            if (IsQuestObjective(beholder))
                displayName.Append(StringHelpers.QuestPrefix);
            if (beholder.CanSee(this))
                displayName.Append(DisplayName);
            // TODO: if player1 (without quest q1) is looking at player2 (with quest q1 and quest item qi1), player1 should not receive something
            else if (capitalizeFirstLetter)
                displayName.Append("Something");
            else
                displayName.Append("something");
            if (beholder.ImpersonatedBy is IAdmin)
                displayName.Append($"[Id: {Blueprint.Id}]");
            return displayName.ToString();
        }

        public override string RelativeDescription(ICharacter beholder) // Add (Quest) to description if beholder is on a quest with 'this' as objective
        {
            StringBuilder description = new StringBuilder();
            if (IsQuestObjective(beholder))
                description.Append(StringHelpers.QuestPrefix);
            description.Append(Description);
            return description.ToString();
        }

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

        public IReadOnlyDictionary<string, string> ExtraDescriptions => Blueprint.ExtraDescriptions;

        public bool IsWearable { get; protected set; } // TODO:

        public int DecayPulseLeft { get; protected set; } // 0: means no decay

        public virtual int Weight { get; }

        public virtual int Cost { get; }

        public virtual bool IsQuestObjective(ICharacter questingCharacter)
        {
            return false; // by default, an item is not a quest objective
        }

        public virtual bool ChangeContainer(IContainer container)
        {
            Log.Default.WriteLine(LogLevels.Info, "ChangeContainer: {0} : {1} -> {2}", DebugName, ContainedInto == null ? "<<??>>" : ContainedInto.DebugName, container == null ? "<<??>>" : container.DebugName);

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
