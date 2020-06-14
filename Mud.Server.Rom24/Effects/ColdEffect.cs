using Mud.Common;
using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Effects
{
    public class ColdEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
    {
        private IRandomManager RandomManager { get; }
        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public ColdEffect(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
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
            // chill touch effect
            if (!victim.SavesSpell(level / 4 + modifier / 20, SchoolTypes.Cold))
            {
                victim.Send("A chill sinks deep into your bones.");
                victim.Act(ActOptions.ToRoom, "{0:N} turns blue and shivers.", victim);
                IAura chillTouchAura = victim.GetAura(auraName);
                if (chillTouchAura != null)
                {
                    chillTouchAura.Update(level, TimeSpan.FromMinutes(6));
                    chillTouchAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.Strength,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                        x => x.Modifier -= 1);
                }
                else
                    AuraManager.AddAura(victim, auraName, source, level, TimeSpan.FromMinutes(6), AuraFlags.None, false,
                        new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
            }
            // hunger! (warmth sucked out)
            (victim as IPlayableCharacter)?.GainCondition(Conditions.Hunger, modifier / 20);
            // let's toast some gear
            IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).ToList());
            foreach (IItem itemOnVictim in clone)
                Apply(itemOnVictim, source, auraName, level, modifier);
            victim.Recompute();
        }

        public void Apply(IItem item, IEntity source, string auraName, int level, int modifier)
        {
            if (!item.IsValid)
                return;
            if (item.ItemFlags.HasFlag(ItemFlags.BurnProof)
                || item.ItemFlags.HasFlag(ItemFlags.NoPurge)
                || RandomManager.Range(0, 4) == 0)
                return;
            // Affects only potion and drink container
            if (!(item is IItemPotion || item is IItemDrinkContainer))
                return;
            int chance = level / 4 + modifier / 10;
            if (chance > 25)
                chance = (chance - 25) / 2 + 25;
            if (chance > 50)
                chance = (chance - 50) / 2 + 50;
            if (item.ItemFlags.HasFlag(ItemFlags.Bless))
                chance -= 5;
            chance -= item.Level * 2;
            if (item is IItemPotion)
                chance += 25;
            else if (item is IItemDrinkContainer)
                chance += 5;
            chance = chance.Range(5, 95);
            if (RandomManager.Range(1, 100) > chance)
                return;
            // display msg
            string msg = "{0} freezes and shatters!";
            ICharacter viewer = (item.ContainedInto as ICharacter) ?? item.EquippedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
            viewer?.Act(ActOptions.ToAll, msg, item);
            // unequip and destroy item
            IEntity itemContainedInto;
            if (item.EquippedBy != null) // if item equipped: unequip 
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
