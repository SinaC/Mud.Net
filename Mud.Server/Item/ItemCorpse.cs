using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Item
{
    public class ItemCorpse : ItemBase<ItemCorpseBlueprint, ItemCorpseData>, IItemCorpse
    {
        private readonly string _corpseName;
        private readonly List<IItem> _content;
        private readonly bool _hasBeenGeneratedByKillingCharacter;

        private IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
            : base(guid, blueprint, BuildName(victim.DisplayName, false, blueprint), BuildShortDescription(victim.DisplayName, false, blueprint), BuildDescription(victim.DisplayName, false, blueprint), room)
        {
            _content = new List<IItem>();
            _corpseName = victim.DisplayName;
            _hasBeenGeneratedByKillingCharacter = true;

            IsPlayableCharacterCorpse = victim is IPlayableCharacter;

            if (IsPlayableCharacterCorpse)
            {
                DecayPulseLeft = RandomManager.Range(25, 40) * Pulse.PulsePerMinutes;
                BaseItemFlags |= ItemFlags.NoPurge; // was handled in object description in limbo.are
                NoTake = true;
            }
            else
            {
                DecayPulseLeft = RandomManager.Range(3, 6) * Pulse.PulsePerMinutes;
                NoTake = false;
            }

            // Money
            if (victim.SilverCoins > 0 || victim.GoldCoins > 0)
            {
                long silver = victim.SilverCoins;
                long gold = victim.GoldCoins;
                if ((silver > 1 || gold > 1)
                    && victim is IPlayableCharacter pcVictim) // player keep half their money
                {
                    silver /= 2;
                    gold /= 2;
                    pcVictim.UpdateMoney(-silver, -gold);
                }

                World.AddItemMoney(Guid.NewGuid(), silver, gold, this);
            }

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
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(this);
                }
                else if (result == PerformActionOnItemResults.MoveToRoom)
                {
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(victim.Room);
                }
                else
                    World.RemoveItem(item);
            }

            // Check victim loot table (only if victim is NPC)
            if (victim is INonPlayableCharacter npcVictim)
            {
                List<int> loots = npcVictim.Blueprint?.LootTable?.GenerateLoots();
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

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, ItemCorpseData itemCorpseData, IContainer container)
            : base(guid, blueprint, itemCorpseData, BuildName(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), BuildShortDescription(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), BuildDescription(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), container)
        {
            _content = new List<IItem>();
            _corpseName = itemCorpseData.CorpseName;
            _hasBeenGeneratedByKillingCharacter = itemCorpseData.HasBeenGeneratedByKillingCharacter;

            IsPlayableCharacterCorpse = itemCorpseData.IsPlayableCharacterCorpse;
            if (itemCorpseData.Contains?.Length > 0)
            {
                foreach (ItemData itemData in itemCorpseData.Contains)
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
        }

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IContainer container)
            : base(guid, blueprint, container)
        {
            _content = new List<IItem>();
            _corpseName = null;
            _hasBeenGeneratedByKillingCharacter = false;
            IsPlayableCharacterCorpse = false;
        }

        #region IItemCorpse

        #region IContainer

        public IEnumerable<IItem> Content => _content;

        public bool PutInContainer(IItem obj)
        {
            _content.Add(obj);
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
                CorpseName = _corpseName,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                Auras = MapAuraData(),
                Contains = MapContent(),
                IsPlayableCharacterCorpse = IsPlayableCharacterCorpse,
                HasBeenGeneratedByKillingCharacter = _hasBeenGeneratedByKillingCharacter,
            };
        }

        #endregion

        private static string BuildName(string corpseName, bool generated, ItemCorpseBlueprint blueprint) 
            => generated
                ? blueprint.Name
                : "corpse " + corpseName;
        private static string BuildShortDescription(string corpseName, bool generated, ItemCorpseBlueprint blueprint)
            => generated
                ? blueprint.ShortDescription
                : "the corpse of " + corpseName;
        private static string BuildDescription(string corpseName, bool generated, ItemCorpseBlueprint blueprint)
            => generated
                ? blueprint.Description
             : $"The corpse of {corpseName} is lying here.";

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
            if (item is IItemPotion)
                item.SetTimer(TimeSpan.FromMinutes(RandomManager.Range(500,1000)));
            else if (item is IItemScroll)
                item.SetTimer(TimeSpan.FromMinutes(RandomManager.Range(1000, 2500)));
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
