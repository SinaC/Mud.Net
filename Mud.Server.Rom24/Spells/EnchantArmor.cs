using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Enchantment, PulseWaitTime = 24)]
    public class EnchantArmor : ItemInventorySpellBase<IItemArmor>
    {
        public const string SpellName = "Enchant Armor";

        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public EnchantArmor(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            ItemManager = itemManager;
        }

        protected override string InvalidItemTypeMsg => "That isn't an armor.";

        protected override void Invoke()
        {
            IItemArmor armor = Item;
            //if (item.EquippedBy == null)
            //{
            //    caster.Send("The item must be carried to be enchanted.");
            //    return;
            //}

            IAura existingAura = null;
            int fail = 25; // base 25% chance of failure

            // find existing bonuses
            foreach (IAura aura in armor.Auras)
            {
                if (aura.AbilityName == SpellName)
                    existingAura = aura;
                bool found = false;
                foreach (CharacterAttributeAffect characterAttributeAffect in aura.Affects.OfType<CharacterAttributeAffect>().Where(x => x.Location == CharacterAttributeAffectLocations.AllArmor))
                {
                    fail += 5 * (characterAttributeAffect.Modifier * characterAttributeAffect.Modifier);
                    found = true;
                }
                if (!found) // things get a little harder
                    fail += 20;
            }
            // apply other modifiers
            fail -= Level;
            if (armor.ItemFlags.HasFlag(ItemFlags.Bless))
                fail -= 15;
            if (armor.ItemFlags.HasFlag(ItemFlags.Glowing))
                fail -= 5;
            fail = fail.Range(5, 85);
            // the moment of truth
            int result = RandomManager.Range(1, 100);
            if (result < fail / 5) // item destroyed
            {
                Caster.Act(ActOptions.ToAll, "{0} flares blindingly... and evaporates!", armor);
                ItemManager.RemoveItem(armor);
                return;
            }
            if (result < fail / 3) // item disenchanted
            {
                Caster.Act(ActOptions.ToCharacter, "{0} glows brightly, then fades...oops.!", armor);
                Caster.Act(ActOptions.ToRoom, "{0} glows brightly, then fades.", armor);
                armor.Disenchant();
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
                Caster.Act(ActOptions.ToAll, "{0} shimmers with a gold aura.", armor);
                amount = -1;
            }
            else // exceptional enchant
            {
                Caster.Act(ActOptions.ToAll, "{0} glows a brillant gold!", armor);
                addGlowing = true;
                amount = -2;
            }
            armor.IncreaseLevel();

            if (existingAura != null)
            {
                existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.AllArmor,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = amount, Operator = AffectOperators.Add },
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
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = amount, Operator = AffectOperators.Add },
                    new ItemFlagsAffect { Modifier = ItemFlags.Magic, Operator = AffectOperators.Or }
                };
                if (addGlowing)
                    affects.Add(new ItemFlagsAffect { Modifier = ItemFlags.Glowing, Operator = AffectOperators.Or });
                AuraManager.AddAura(armor, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent, false,
                   affects.ToArray());
            }
            armor.Recompute();
        }
    }
}
