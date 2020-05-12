using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;

namespace Mud.Server.Item
{
    public class ItemCorpse : ItemBase<ItemCorpseBlueprint>, IItemCorpse
    {
        private readonly string _corpseName;
        private readonly List<IItem> _content;

        private IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();

        public override string DisplayName => "The corpse of " + _corpseName;

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
            : base(guid, blueprint, room)
        {
            _corpseName = victim.DisplayName;
            Name = "corpse of " + _corpseName;
            // TODO: custom short description
            Description = "The corpse of " + _corpseName + " is laying here.";
            _content = new List<IItem>();

            IsPlayableCharacterCorpse = victim is IPlayableCharacter;

            if (IsPlayableCharacterCorpse)
            {
                DecayPulseLeft = RandomManager.Range(25, 40) * Pulse.PulsePerMinutes;
                BaseItemFlags |= ItemFlags.NoPurge;
            }
            else
                DecayPulseLeft = RandomManager.Range(3, 6) * Pulse.PulsePerMinutes;

            // TODO: money

            // Fill corpse with inventory
            IReadOnlyCollection<IItem> inventory = new ReadOnlyCollection<IItem>(victim.Inventory.ToList());
            foreach (IItem item in inventory)
            {
                var result = PerformActionOnItem(victim, item);
                if (result == PerformActionOnItemResults.MoveToCorpse)
                    item.ChangeContainer(this);
                else if (result == PerformActionOnItemResults.MoveToRoom)
                    item.ChangeContainer(victim.Room);
                else
                    World.RemoveItem(item);
            }
            // Fill corpse with equipment
            IReadOnlyCollection<IItem> equipment = new ReadOnlyCollection<IItem>(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item).ToList());
            foreach (IItem item in equipment)
            {
                var result = PerformActionOnItem(victim, item);
                if (result == PerformActionOnItemResults.MoveToCorpse)
                {
                    item.ChangeEquippedBy(null);
                    item.ChangeContainer(this);
                }
                else if (result == PerformActionOnItemResults.MoveToRoom)
                {
                    item.ChangeEquippedBy(null);
                    item.ChangeContainer(victim.Room);
                }
                else
                    World.RemoveItem(item);
            }

            // Check victim loot table (only if victim is NPC)
            if (victim is INonPlayableCharacter nonPlayableCharacterVictom)
            {
                List<int> loots = nonPlayableCharacterVictom.Blueprint?.LootTable?.GenerateLoots();
                if (loots != null && loots.Any())
                {
                    foreach (int loot in loots)
                        World.AddItem(Guid.NewGuid(), loot, this);
                }
            }
        }

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer)
            : this(guid, blueprint, room, victim)
        {
            // Check killer quest table (only if killer is PC and victim is NPC) // TODO: only visible for people on quest???
            if (killer != null && killer is IPlayableCharacter playableCharacterKiller && victim is INonPlayableCharacter nonPlayableCharacterVictim)
            {
                foreach (IQuest quest in playableCharacterKiller.Quests)
                {
                    // Update kill objectives
                    quest.Update(nonPlayableCharacterVictim);
                    // Generate loot on corpse
                    quest.GenerateKillLoot(nonPlayableCharacterVictim, this);
                }
            }
        }

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, ItemCorpseData itemCorpseData, IContainer containedInto)
            : base(guid, blueprint, itemCorpseData, containedInto)
        {
            _corpseName = itemCorpseData.CorpseName;
            Name = "corpse of " + _corpseName;
            // TODO: custom short description
            Description = "The corpse of " + _corpseName + " is laying here.";
            _content = new List<IItem>();

            IsPlayableCharacterCorpse = itemCorpseData.IsPlayableCharacterCorpse;
            if (itemCorpseData.Contains?.Length > 0)
            {
                foreach (ItemData itemData in itemCorpseData.Contains)
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
        }

        #region IItemCorpse

        #region IContainer

        public IEnumerable<IItem> Content => _content;

        public bool PutInContainer(IItem obj)
        {
            //return false; // cannot put anything in a corpse, puttin something is done thru constructor
            _content.Insert(0, obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            return _content.Remove(obj);
        }

        #endregion

        public bool IsPlayableCharacterCorpse { get; protected set; }

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemCorpseData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                Auras = MapAuraData(),
                Contains = MapContent(),
                CorpseName = _corpseName,
                IsPlayableCharacterCorpse = IsPlayableCharacterCorpse
            };
        }

        #endregion

        // Perform actions on item before putting it in corpse
        // returns false if item must be destroyed instead of being put in corpse
        public enum PerformActionOnItemResults
        {
            MoveToCorpse,
            MoveToRoom,
            Destroy,
        }
        private PerformActionOnItemResults PerformActionOnItem(ICharacter victim, IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.Inventory))
                return PerformActionOnItemResults.Destroy;
            // TODO: check stay death flag
            // TODO: if potion: timer 500->1000
            // TODO: if scroll: timer 1000->2500
            if (item.ItemFlags.HasFlag((ItemFlags.VisibleDeath)))
                item.RemoveBaseItemFlags(ItemFlags.VisibleDeath);
            bool isFloating = item.WearLocation == WearLocations.Float;
            if (isFloating)
            {
                if (item.ItemFlags.HasFlag(ItemFlags.RotDeath))
                {
                    if (item is IItemContainer container && container.Content.Any())
                    {
                        victim.Act(ActOptions.ToRoom, "{0} evaporates, scattering its contents.", item);
                        var cloneContent = new ReadOnlyCollection<IItem>(container.Content.ToList());
                        foreach (IItem contentItem in cloneContent)
                            contentItem.ChangeContainer(victim.Room);
                    }
                    else
                        victim.Act(ActOptions.ToRoom, "{0} evaporates.", item);
                    return PerformActionOnItemResults.Destroy;
                }
                victim.Act(ActOptions.ToRoom, "{0} falls to the floor.", item);
                return PerformActionOnItemResults.MoveToRoom;
            }
            if (item.ItemFlags.HasFlag(ItemFlags.RotDeath))
            {
                int duration = RandomManager.Range(5, 10);
                item.SetTimer(TimeSpan.FromMinutes(duration));
                item.RemoveBaseItemFlags(ItemFlags.RotDeath);
            }
            return PerformActionOnItemResults.MoveToCorpse;
        }

        private ItemData[] MapContent()
        {
            if (Content.Any())
                return Content.Select(x => x.MapItemData()).ToArray();
            return null;
        }
    }
}
