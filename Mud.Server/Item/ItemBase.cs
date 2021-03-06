﻿using System;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Entity;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Item
{
    public abstract class ItemBase<TBlueprint, TData> : EntityBase, IItem
        where TBlueprint : ItemBlueprintBase
        where TData: ItemData
    {
        protected IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();
        protected IAuraManager AuraManager => DependencyContainer.Current.GetInstance<IAuraManager>();

        protected ItemBase(Guid guid, TBlueprint blueprint, string name, string shortDescription, string description, IContainer containedInto)
            : base(guid, name, description)
        {
            Blueprint = blueprint;
            ShortDescription = shortDescription;
            containedInto.PutInContainer(this); // put in container
            ContainedInto = containedInto; // set above container as our container
            WearLocation = blueprint.WearLocation;
            Level = blueprint.Level;
            Weight = blueprint.Weight;
            Cost = blueprint.Cost;
            NoTake = blueprint.NoTake;
            BaseItemFlags = NewAndCopyAndSet<IItemFlags, IItemFlagValues>(() => new ItemFlags(), blueprint.ItemFlags, null);
            ItemFlags = NewAndCopyAndSet<IItemFlags, IItemFlagValues>(() => new ItemFlags(), BaseItemFlags, null);
        }

        protected ItemBase(Guid guid, TBlueprint blueprint, IContainer containedInto)
            : this(guid, blueprint, blueprint.Name, blueprint.ShortDescription, blueprint.Description, containedInto)
        {
        }

        protected ItemBase(Guid guid, TBlueprint blueprint, TData data, string name, string shortDescription, string description, IContainer containedInto)
            : this(guid, blueprint, name, shortDescription, description, containedInto)
        {
            Level = data.Level;
            DecayPulseLeft = data.DecayPulseLeft;
            BaseItemFlags = NewAndCopyAndSet<IItemFlags, IItemFlagValues>(() => new ItemFlags(), data.ItemFlags, null);
            ItemFlags = NewAndCopyAndSet<IItemFlags, IItemFlagValues>(() => new ItemFlags(), BaseItemFlags, null);
            // Auras
            if (data.Auras != null)
            {
                foreach (AuraData auraData in data.Auras)
                    AuraManager.AddAura(this, auraData, false);
            }
        }

        protected ItemBase(Guid guid, TBlueprint blueprint, TData data, IContainer containedInto)
            : this(guid, blueprint, containedInto)
        {
            Level = data.Level;
            DecayPulseLeft = data.DecayPulseLeft;
            BaseItemFlags = NewAndCopyAndSet<IItemFlags, IItemFlagValues>(() => new ItemFlags(), data.ItemFlags, null);
            ItemFlags = NewAndCopyAndSet<IItemFlags, IItemFlagValues>(() => new ItemFlags(), BaseItemFlags, null);
            // Auras
            if (data.Auras != null)
            {
                foreach (AuraData auraData in data.Auras)
                    AuraManager.AddAura(this, auraData, false);
            }
        }

        #region IItem

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemBase<TBlueprint, TData>>();

        #endregion

        public override string DisplayName => ShortDescription ?? Blueprint.ShortDescription ?? Name.UpperFirstLetter();

        public override string DebugName => Blueprint == null ? DisplayName : $"{DisplayName}[{Blueprint.Id}]";

        // Recompute
        public override void ResetAttributes()
        {
            ItemFlags.Copy(BaseItemFlags);
        }

        public override void Recompute()
        {
            Log.Default.WriteLine(LogLevels.Debug, "ItemBase.Recompute: {0}", DebugName);

            // 0) Reset
            ResetAttributes();

            // 1) Apply auras from room containing item if in a room
            if (ContainedInto is IRoom room && room.IsValid)
            {
                ApplyAuras<IItem>(room, this);
            }

            // 2) Apply auras from character equiping item if equipped by a character
            if (EquippedBy != null && EquippedBy.IsValid)
            {
                ApplyAuras<IItem>(EquippedBy, this);
            }

            // 3) Apply own auras
            ApplyAuras<IItem>(this, this);
        }

        //
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
            if (playableBeholder?.IsImmortal == true)
                displayName.Append($" [id: {Blueprint?.Id.ToString() ?? " ??? "}]");
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
            ContainedInto = RoomManager.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
        }

        public override void OnCleaned() // called when removing definitively an entity from the game
        {
            Blueprint = null;
            ContainedInto = null;
        }

        #endregion

        public IContainer ContainedInto { get; private set; }

        public ItemBlueprintBase Blueprint { get; private set; }

        public string ShortDescription { get; protected set; }

        public ILookup<string, string> ExtraDescriptions => Blueprint.ExtraDescriptions;

        public WearLocations WearLocation { get; }

        public ICharacter EquippedBy { get; private set; }

        public int DecayPulseLeft { get; protected set; } // 0: means no decay

        public int Level { get; protected set; }

        public int Weight { get; }

        public int Cost { get; }

        public bool NoTake { get; protected set; }

        public virtual int TotalWeight => Weight;

        public virtual int CarryCount => 1;

        public IItemFlags BaseItemFlags { get; protected set; }

        public IItemFlags ItemFlags { get; protected set; }

        public virtual bool IsQuestObjective(IPlayableCharacter questingCharacter)
        {
            return false; // by default, an item is not a quest objective
        }

        public virtual bool ChangeContainer(IContainer container)
        {
            if (container == this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to put a container in itself!!");
                return false;
            }

            Log.Default.WriteLine(LogLevels.Info, "ChangeContainer: {0} : {1} -> {2}", DebugName, ContainedInto == null ? "<<??>>" : ContainedInto.DebugName, container == null ? "<<??>>" : container.DebugName);

            ContainedInto?.GetFromContainer(this);
            //Debug.Assert(container != null, "ChangeContainer: an item cannot be outside a container"); // False, equipment are not stored in any container
            bool putInContainer = container?.PutInContainer(this) ?? true;
            if (!putInContainer)
                return false;
            ContainedInto = container;

            return true;
        }

        public bool ChangeEquippedBy(ICharacter character, bool recompute)
        {
            ICharacter previousEquippedBy = EquippedBy;
            EquippedBy?.Unequip(this);
            Log.Default.WriteLine(LogLevels.Info, "ChangeEquippedBy: {0} : {1} -> {2}", DebugName, EquippedBy?.DebugName ?? "<<??>>", character?.DebugName ?? "<<??>>");
            EquippedBy = character;
            character?.Equip(this);
            if (recompute)
                previousEquippedBy?.Recompute();
            if (recompute)
                EquippedBy?.Recompute();
            return true;
        }

        public void DecreaseDecayPulseLeft(int pulseCount)
        {
            if (DecayPulseLeft > 0)
                DecayPulseLeft = Math.Max(0, DecayPulseLeft - pulseCount);
        }

        public void SetTimer(TimeSpan duration)
        {
            DecayPulseLeft = Pulse.FromTimeSpan(duration);
        }

        public void AddBaseItemFlags(bool recompute, params string[] flags)
        {
            BaseItemFlags.Set(flags);
            if (recompute)
                Recompute();
        }

        public void RemoveBaseItemFlags(bool recompute, params string[] flags)
        {
            BaseItemFlags.Unset(flags);
            if (recompute)
                Recompute();
        }

        public void Disenchant()
        {
            RemoveAuras(_ => true, false);
            BaseItemFlags = new ItemFlags();
            Recompute();
        }

        public void IncreaseLevel()
        {
            Level++;
        }

        public virtual StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay)
        {
            // Item flags
            if (ItemFlags.IsSet("Invis") && viewer.CharacterFlags.IsSet("DetectInvis")) sb.Append("%y%(Invis)%x%");
            if (ItemFlags.IsSet("Evil") && viewer.CharacterFlags.IsSet("DetectEvil")) sb.Append("%R%(Evil)%x%");
            if (ItemFlags.IsSet("Bless") && viewer.CharacterFlags.IsSet("DetectGood")) sb.Append("%C%(Blessed)%x%");
            if (ItemFlags.IsSet("Magic") && viewer.CharacterFlags.IsSet("DetectMagic")) sb.Append("%b%(Magical)%x%");
            if (ItemFlags.IsSet("Glowing")) sb.Append("%Y%(Glowing)%x%");
            if (ItemFlags.IsSet("Humming")) sb.Append("%y%(Humming)%x%");

            // Description
            if (shortDisplay)
                sb.Append(RelativeDisplayName(viewer));
            else
                sb.Append(RelativeDescription(viewer));
            //
            return sb;
        }

        public void ApplyAffect(IItemFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    ItemFlags.Set(affect.Modifier);
                    break;
                case AffectOperators.Assign:
                    ItemFlags.Copy(affect.Modifier);
                    break;
                case AffectOperators.Nor:
                    ItemFlags.Unset(affect.Modifier);
                    break;
            }
        }

        public virtual ItemData MapItemData()
        {
            return new ItemData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags, // Current will be recompute with auras
                Auras = MapAuraData()
            };
        }

        #endregion

        protected void ApplyAuras<T>(IEntity source, T target)
            where T : IItem
        {
            if (!source.IsValid)
                return;
            foreach (IAura aura in source.Auras.Where(x => x.IsValid))
            {
                foreach (IItemAffect<T> affect in aura.Affects.OfType<IItemAffect<T>>())
                {
                    affect.Apply(target);
                }
            }
        }
    }
}
