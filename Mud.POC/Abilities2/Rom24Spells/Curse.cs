using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Curse", AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("The curse wears off.")]
    [AbilityItemWearOffMessage("{0} is no longer impure.")]
    [AbilityDispellable("{0} is no longer impure.")]
    public class Curse : ItemOrOffensiveSpellBase
    {
        private IAuraManager AuraManager { get; }
        public Curse(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            IAura curseAura = victim.GetAura("Curse");
            if (curseAura != null || victim.CharacterFlags.HasFlag(CharacterFlags.Curse) || victim.SavesSpell(Level, SchoolTypes.Negative))
                return;
            victim.Send("You feel unclean.");
            if (Caster != victim)
                Caster.Act(ActOptions.ToCharacter, "{0:N} looks very uncomfortable.", victim);
            int duration = 2 * Level;
            int modifier = Level / 8;
            AuraManager.AddAura(victim, this, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Curse, Operator = AffectOperators.Or });
        }

        protected override void Invoke(IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.Evil))
            {
                Caster.Act(ActOptions.ToCharacter, "{0} is already filled with evil.", item);
                return;
            }
            if (item.ItemFlags.HasFlag(ItemFlags.Bless))
            {
                IAura blessAura = item.GetAura("Bless");
                if (!SavesDispel(Level, blessAura?.Level ?? item.Level, 0))
                {
                    if (blessAura != null)
                        item.RemoveAura(blessAura, false);
                    Caster.Act(ActOptions.ToAll, "{0} glows with a red aura.", item);
                    item.RemoveBaseItemFlags(ItemFlags.Bless);
                    return;
                }
                else
                    Caster.Act(ActOptions.ToCharacter, "The holy aura of {0} is too powerful for you to overcome.");
                return;
            }
            AuraManager.AddAura(item, this, Caster, Level, TimeSpan.FromMinutes(2 * Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = 1, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = ItemFlags.Evil, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0} glows with a malevolent aura.", item);
        }
    }
}
