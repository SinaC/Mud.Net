using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Bless", AbilityEffects.Buff | AbilityEffects.Dispel)]
    [AbilityCharacterWearOffMessage("You feel less righteous.")]
    [AbilityItemWearOffMessage("{0}'s holy aura fades.")]
    public class Bless : ItemOrDefensiveSpellBase
    {
        private IAuraManager AuraManager { get; }
        public Bless(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager) 
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            IAura blessAura = victim.GetAura("Bless");
            if (victim.Position == Positions.Fighting || blessAura != null)
            {
                if (Caster == victim)
                    Caster.Send("You are already blessed.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} already has divine favor.", victim);
                return;
            }
            victim.Send("You feel righteous.");
            if (victim != Caster)
                Caster.Act(ActOptions.ToCharacter, "You grant {0} the favor of your god.", victim);
            int duration = 6 + Level;
            AuraManager.AddAura(victim, this, Caster, Level, TimeSpan.FromHours(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = Level / 8, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -Level / 8, Operator = AffectOperators.Add });
        }

        protected override void Invoke(IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.Bless))
            {
                Caster.Act(ActOptions.ToCharacter, "{0:N} is already blessed.", item);
                return;
            }
            if (item.ItemFlags.HasFlag(ItemFlags.Evil))
            {
                IAura evilAura = item.GetAura("Curse");
                if (!SavesDispel(Level, evilAura?.Level ?? item.Level, 0))
                {
                    if (evilAura != null)
                        item.RemoveAura(evilAura, false);
                    Caster.Act(ActOptions.ToAll, "{0} glows a pale blue.", item);
                    item.RemoveBaseItemFlags(ItemFlags.Evil);
                    return;
                }
                Caster.Act(ActOptions.ToCharacter, "The evil of {0} is too powerful for you to overcome.", item);
                return;
            }
            AuraManager.AddAura(item, this, Caster, Level, TimeSpan.FromHours(6 + Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = ItemFlags.Bless, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0} glows with a holy aura.", item);
        }
    }
}
