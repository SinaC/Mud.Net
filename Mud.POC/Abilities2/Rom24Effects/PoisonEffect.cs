﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Effects
{
    public class PoisonEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
    {
        private IRandomManager RandomManager { get; }
        private IAuraManager AuraManager { get; }

        public PoisonEffect(IRandomManager randomManager, IAuraManager auraManager)
        {
            RandomManager = randomManager;
            AuraManager = auraManager;
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
            // chance of poisoning
            if (!victim.SavesSpell(level / 4 + modifier / 20, SchoolTypes.Poison))
            {
                victim.Send("You feel poison coursing through your veins.");
                victim.Act(ActOptions.ToRoom, "{0} looks very ill.", victim);
                int duration = level / 2;
                IAura poisonAura = victim.GetAura(auraName);
                if (poisonAura != null)
                {
                    poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                    poisonAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.Strength,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                        x => x.Modifier -= 1);
                }
                else
                    AuraManager.AddAura(victim, auraName, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                        new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                        new PoisonDamageAffect());
            }
            // equipment
            IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).ToList());
            foreach (IItem itemOnVictim in clone)
                Apply(itemOnVictim, source, auraName, level, modifier);
            victim.Recompute();
        }

        public void Apply(IItem item, IEntity source, string auraName, int level, int modifier)
        {
            if (!item.IsValid)
                return;
            IItemPoisonable poisonable = item as IItemPoisonable;
            if (poisonable == null)
                return;
            if (poisonable.ItemFlags.HasFlag(ItemFlags.BurnProof)
                || poisonable.ItemFlags.HasFlag(ItemFlags.Bless)
                || RandomManager.Chance(25))
                return;
            int chance = level / 4 + modifier / 10;
            if (chance > 25)
                chance = (chance - 25) / 2 + 25;
            if (chance > 50)
                chance = (chance - 50) / 2 + 50;
            if (poisonable.ItemFlags.HasFlag(ItemFlags.Bless))
                chance -= 5;
            chance -= poisonable.Level * 2;
            chance = chance.Range(5, 95);
            if (!RandomManager.Chance(chance))
                return;
            poisonable.Poison();
        }
    }
}
