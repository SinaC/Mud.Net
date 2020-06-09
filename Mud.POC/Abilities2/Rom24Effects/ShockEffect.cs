using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Effects
{
    public class ShockEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
    {
        private IRandomManager RandomManager { get; }
        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public ShockEffect(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
        {
            RandomManager = randomManager;
            AuraManager = auraManager;
            ItemManager = itemManager;
        }

        public void Apply(IRoom room, IEntity source, string auraName, int level, int modifier)
        {
            if (!room.IsValid)
                return;
            IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(room.Content.ToList());
            foreach (IItem itemInRoom in clone)
                Apply(itemInRoom, source, auraName, level, modifier);
            room.Recompute();
        }

        public void Apply(ICharacter victim, IEntity source, string auraName, int level, int modifier)
        {
            if (!victim.IsValid)
                return;
            if (!victim.SavesSpell(level / 4 + modifier / 20, SchoolTypes.Lightning))
            {
                victim.Send("Your muscles stop responding.");
                // TODO: set Daze to Math.Max(12, level/4 + modifier/20)
            }
            // toast some gear
            IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).ToList());
            foreach (IItem itemOnVictim in clone)
                Apply(itemOnVictim, source, auraName, level, modifier);
            victim.Recompute();
        }

        public void Apply(IItem item, IEntity source, string auraName, int level, int modifier)
        {
            if (!item.IsValid)
                return;
            int chance = level / 4 + modifier / 10;
            if (chance > 25)
                chance = (chance - 25) / 2 + 25;
            if (chance > 50)
                chance = (chance - 50) / 2 + 50;
            if (item.ItemFlags.HasFlag(ItemFlags.Bless))
                chance -= 5;
            chance -= item.Level * 2;
            chance = chance.Range(5, 95);
            if (RandomManager.Range(1, 100) > chance)
                return;
            // unequip and destroy item
            IEntity itemContainedInto;
            if (item.EquippedBy != null) // if item is equipped: unequip 
            {
                item.ChangeEquippedBy(null, true);
                itemContainedInto = item.EquippedBy;
            }
            else
                itemContainedInto = item.ContainedInto;
            //
            ItemManager.RemoveItem(item); // destroy item
            itemContainedInto?.Recompute();
        }
    }
}
