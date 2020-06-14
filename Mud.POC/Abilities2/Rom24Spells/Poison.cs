using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You feel less sick.")]
    [AbilityItemWearOffMessage("The poison on {0} dries up.")]
    public class Poison : ItemOrOffensiveSpellBase
    {
        public const string SpellName = "Poison";

        private IAuraManager AuraManager { get; }

        public Poison(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            if (victim.SavesSpell(Level, SchoolTypes.Poison))
            {
                victim.Act(ActOptions.ToRoom, "{0:N} turns slightly green, but it passes.", victim);
                victim.Send("You feel momentarily ill, but it passes.");
                return;
            }

            int duration = Level;
            IAura poisonAura = victim.GetAura(SpellName);
            if (poisonAura != null)
                poisonAura.Update(Level, TimeSpan.FromMinutes(duration));
            else
                AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                    new PoisonDamageAffect());
            victim.Send("You feel very sick.");
            victim.Act(ActOptions.ToRoom, "{0:N} looks very ill.", victim);
        }

        protected override void Invoke(IItem item)
        {
            // food/drink container
            if (item is IItemPoisonable poisonable)
            {
                if (poisonable.ItemFlags.HasFlag(ItemFlags.Bless) || poisonable.ItemFlags.HasFlag(ItemFlags.BurnProof))
                {
                    Caster.Act(ActOptions.ToCharacter, "Your spell fails to corrupt {0}.", poisonable);
                    return;
                }
                poisonable.Poison();
                Caster.Act(ActOptions.ToCharacter, "{0} is infused with poisonous vapors.", poisonable);
                return;
            }
            // weapon
            if (item is IItemWeapon weapon)
            {
                if (weapon.WeaponFlags == WeaponFlags.Poison)
                {
                    Caster.Act(ActOptions.ToCharacter, "{0} is already envenomed.", weapon);
                    return;
                }
                if (weapon.WeaponFlags != WeaponFlags.None)
                {
                    Caster.Act(ActOptions.ToCharacter, "You can't seem to envenom {0}.", weapon);
                    return;
                }
                int duration = Level / 8;
                AuraManager.AddAura(weapon, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Poison, Operator = AffectOperators.Or });
                Caster.Act(ActOptions.ToCharacter, "{0} is coated with deadly venom.", weapon);
                return;
            }
            Caster.Act(ActOptions.ToCharacter, "You can't poison {0}.", item);
        }
    }
}
