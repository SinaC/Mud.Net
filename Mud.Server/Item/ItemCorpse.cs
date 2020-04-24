using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemCorpse : ItemBase<ItemCorpseBlueprint>, IItemCorpse
    {
        private readonly string _corpseName;
        private readonly List<IItem> _content;

        public override string DisplayName => "The corpse of " + _corpseName;

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
            : base(guid, blueprint, room)
        {
            _corpseName = victim.DisplayName;
            Name = "corpse of " + _corpseName;
            // TODO: custom short description
            Description = "The corpse of " + _corpseName + " is laying here.";
            _content = new List<IItem>();

            IReadOnlyCollection<IItem> inventory = new ReadOnlyCollection<IItem>(victim.Content.ToList()); // clone
            if (inventory.Any())
                DecayPulseLeft = Settings.PulsePerMinutes * 5;
            else
                DecayPulseLeft = Settings.PulsePerMinutes;
            foreach (IItem item in inventory) // TODO: check stay death flag
                item.ChangeContainer(this);
            foreach (IEquipable item in victim.Equipments.Where(x => x.Item != null).Select(x => x.Item))
            {
                item.ChangeContainer(this);
                item.ChangeEquipedBy(null);
            }

            IsPlayableCharacterCorpse = victim is IPlayableCharacter;

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

        public override int Weight => base.Weight + _content.Sum(x => x.Weight);

        #region IItemCorpse

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemCorpseData
            {
                ItemId = Blueprint.Id,
                DecayPulseLeft = DecayPulseLeft,
                Contains = MapContent(),
                CorpseName = _corpseName,
                IsPlayableCharacterCorpse = IsPlayableCharacterCorpse
            };
        }

        #endregion

        public bool IsPlayableCharacterCorpse { get; protected set; }

        #endregion

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

        private ItemData[] MapContent()
        {
            if (Content.Any())
                return Content.Select(x => x.MapItemData()).ToArray();
            return null;
        }
    }
}
