using System;
using System.Collections.Generic;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Item
{
    public abstract class ItemBase<TBlueprint> : EntityBase, IItem
        where TBlueprint : ItemBlueprintBase
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> ItemCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => GetCommands<ItemBase<TBlueprint>>());

        protected ItemBase(Guid guid, TBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            ContainedInto = containedInto;
            containedInto.PutInContainer(this);
            Weight = blueprint.Weight;
            Cost = blueprint.Cost;
            ItemFlags = blueprint.ItemFlags;
        }

        protected ItemBase(Guid guid, TBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : this(guid, blueprint, containedInto)
        {
            // TODO: copy other fields
            DecayPulseLeft = itemData.DecayPulseLeft;
            ItemFlags = itemData.ItemFlags;
        }

        #region IItem

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => ItemCommands.Value;

        #endregion

        public override string DisplayName => Blueprint == null ? Name.UpperFirstLetter() : Blueprint.ShortDescription;

        public override string DebugName => Blueprint == null ? DisplayName : $"{DisplayName}[{Blueprint.Id}]";

        public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
        {
            StringBuilder displayName = new StringBuilder();
            IPlayableCharacter playableBeholder = beholder as IPlayableCharacter;
            if (playableBeholder != null && IsQuestObjective(playableBeholder))
                displayName.Append(StringHelpers.QuestPrefix);
            if (beholder.CanSee(this))
                displayName.Append(DisplayName);
            else if (capitalizeFirstLetter)
                displayName.Append("Something");
            else
                displayName.Append("something");
            if (playableBeholder?.ImpersonatedBy is IAdmin)
                displayName.Append($" [{Blueprint?.Id.ToString() ?? " ??? "}]");
            return displayName.ToString();
        }

        public override string RelativeDescription(ICharacter beholder) // Add (Quest) to description if beholder is on a quest with 'this' as objective
        {
            StringBuilder description = new StringBuilder();
            if (beholder is IPlayableCharacter playableBeholder && IsQuestObjective(playableBeholder))
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

        public int DecayPulseLeft { get; protected set; } // 0: means no decay

        public virtual int Weight { get; }

        public virtual int Cost { get; }

        public ItemFlags ItemFlags { get; protected set; }


        public virtual bool IsQuestObjective(IPlayableCharacter questingCharacter)
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

        public void DecreaseDecayPulseLeft(int pulseCount)
        {
            if (DecayPulseLeft > 0)
                DecayPulseLeft = Math.Max(0, DecayPulseLeft - pulseCount);
        }

        public virtual ItemData MapItemData()
        {
            return new ItemData
            {
                ItemId = Blueprint.Id,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = ItemFlags
            };
        }

        #endregion
    }
}
