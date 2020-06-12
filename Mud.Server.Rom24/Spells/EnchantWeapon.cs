using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Enchantment, PulseWaitTime = 24)]
    public class EnchantWeapon : ItemInventorySpellBase<IItemWeapon>
    {
        public const string SpellName = "Enchant Weapon";

        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public EnchantWeapon(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            ItemManager = itemManager;
        }

        protected override string InvalidItemTypeMsg => "This is not a weapon.";

        protected override void Invoke()
        {
            IItemWeapon weapon = Item;
            //if (weapon.EquippedBy == null)
            //{
            //    Caster.Send("The item must be carried to be enchanted.");
            //    return;
            //}

            IAura existingAura = null;
            int fail = 25; // base 25% chance of failure

            // find existing bonuses
            foreach (IAura aura in weapon.Auras)
            {
                if (aura.AbilityName == SpellName)
                    existingAura = aura;
                bool found = false;
                foreach (CharacterAttributeAffect characterAttributeAffect in aura.Affects.OfType<CharacterAttributeAffect>().Where(x => x.Location == CharacterAttributeAffectLocations.HitRoll || x.Location == CharacterAttributeAffectLocations.DamRoll))
                {
                    fail += 5 * (characterAttributeAffect.Modifier * characterAttributeAffect.Modifier);
                    found = true;
                }
                if (!found) // things get a little harder
                    fail += 20;
            }
            // apply other modifiers
            fail -= 3 * Level / 2;
            if (weapon.ItemFlags.HasFlag(ItemFlags.Bless))
                fail -= 15;
            if (weapon.ItemFlags.HasFlag(ItemFlags.Glowing))
                fail -= 5;
            fail = fail.Range(5, 95);
            // the moment of truth
            int result = RandomManager.Range(1, 100);
            if (result < fail / 5) // item destroyed
            {
                Caster.Act(ActOptions.ToAll, "{0:N} flares blindingly... and evaporates!", weapon);
                ItemManager.RemoveItem(weapon);
                return;
            }
            if (result < fail / 3) // item disenchanted
            {
                Caster.Act(ActOptions.ToCharacter, "{0} glows brightly, then fades...oops.!", weapon);
                Caster.Act(ActOptions.ToRoom, "{0:N} glows brightly, then fades.", weapon);
                weapon.Disenchant();
                return;
            }
            if (result <= fail) // failed, no bad result
            {
                Caster.Send("Nothing seemed to happen.");
                return;
            }
            int amount;
            bool addGlowing = false;
            if (result <= (90 - Level / 5)) // success
            {
                Caster.Act(ActOptions.ToAll, "{0:N} glows blue.", weapon);
                amount = 1;
            }
            else // exceptional enchant
            {
                Caster.Act(ActOptions.ToAll, "{0:N} glows a brillant blue!", weapon);
                addGlowing = true;
                amount = 2;
            }
            weapon.IncreaseLevel();
            if (existingAura != null)
            {
                existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.HitRoll,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = amount, Operator = AffectOperators.Add },
                        x => x.Modifier += amount);
                existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.DamRoll,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = amount, Operator = AffectOperators.Add },
                        x => x.Modifier += amount);
                existingAura.AddOrUpdateAffect(
                        x => x.Modifier == ItemFlags.Magic,
                        () => new ItemFlagsAffect { Modifier = ItemFlags.Magic, Operator = AffectOperators.Or },
                        x => x.Modifier += amount);
                if (addGlowing)
                    existingAura.AddOrUpdateAffect(
                       x => x.Modifier == ItemFlags.Glowing,
                       () => new ItemFlagsAffect { Modifier = ItemFlags.Glowing, Operator = AffectOperators.Or },
                       x => x.Modifier += amount);
            }
            else
            {
                List<IAffect> affects = new List<IAffect>
                {
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = amount, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = amount, Operator = AffectOperators.Add },
                    new ItemFlagsAffect { Modifier = ItemFlags.Magic, Operator = AffectOperators.Or }
                };
                if (addGlowing)
                    affects.Add(new ItemFlagsAffect { Modifier = ItemFlags.Glowing, Operator = AffectOperators.Or });
                AuraManager.AddAura(weapon, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent, false,
                   affects.ToArray());
            }
            weapon.Recompute();
        }
    }
}
