using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class Bless : ItemOrCharacterBuffSpellBase
    {
        public override int Id => 3;
        public override string Name => "Bless";
        public override AbilityEffects Effects => AbilityEffects.Buff | AbilityEffects.Cure;
        public override string CharacterWearOffMessage => "You feel less righteous.";
        public override string ItemWearOffMessage => "{0}'s holy aura fades.";

        private IAuraManager AuraManager { get; }
        public Bless(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager) 
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            IAura blessAura = victim.GetAura("Bless");
            if (victim.Position == Positions.Fighting || blessAura != null)
            {
                if (caster == victim)
                    caster.Send("You are already blessed.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} already has divine favor.", victim);
                return;
            }
            victim.Send("You feel righteous.");
            if (victim != caster)
                caster.Act(ActOptions.ToCharacter, "You grant {0} the favor of your god.", victim);
            int duration = 6 + level;
            AuraManager.AddAura(victim, this, caster, level, TimeSpan.FromHours(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = level / 8, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -level / 8, Operator = AffectOperators.Add });
        }

        public override void Action(ICharacter caster, int level, IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.Bless))
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} is already blessed.", item);
                return;
            }
            if (item.ItemFlags.HasFlag(ItemFlags.Evil))
            {
                IAura evilAura = item.GetAura("Curse");
                if (!SavesDispel(level, evilAura?.Level ?? item.Level, 0))
                {
                    if (evilAura != null)
                        item.RemoveAura(evilAura, false);
                    caster.Act(ActOptions.ToAll, "{0} glows a pale blue.", item);
                    item.RemoveBaseItemFlags(ItemFlags.Evil);
                    return;
                }
                caster.Act(ActOptions.ToCharacter, "The evil of {0} is too powerful for you to overcome.", item);
                return;
            }
            AuraManager.AddAura(item, this, caster, level, TimeSpan.FromHours(6 + level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = ItemFlags.Bless, Operator = AffectOperators.Or });
            caster.Act(ActOptions.ToAll, "{0} glows with a holy aura.", item);
        }
    }
}
