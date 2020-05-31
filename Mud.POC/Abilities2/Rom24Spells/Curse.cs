using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class Curse : ItemOrCharacterDebuffSpellBase, IAbilityDispellable
    {
        public override int Id => 29;
        public override string Name => "Curse";
        public override AbilityEffects Effects => AbilityEffects.Debuff;
        public override string CharacterWearOffMessage => "The curse wears off.";
        public override string ItemWearOffMessage => "{0} is no longer impure.";
        public string DispelRoomMessage => "{0} is no longer impure.";

        private IAuraManager AuraManager { get; }
        public Curse(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            IAura curseAura = victim.GetAura("Curse");
            if (curseAura != null || victim.CharacterFlags.HasFlag(CharacterFlags.Curse) || victim.SavesSpell(level, SchoolTypes.Negative))
                return;
            victim.Send("You feel unclean.");
            if (caster != victim)
                caster.Act(ActOptions.ToCharacter, "{0:N} looks very uncomfortable.", victim);
            int duration = 2 * level;
            int modifier = level / 8;
            AuraManager.AddAura(victim, this, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Curse, Operator = AffectOperators.Or });
        }

        public override void Action(ICharacter caster, int level, IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.Evil))
            {
                caster.Act(ActOptions.ToCharacter, "{0} is already filled with evil.", item);
                return;
            }
            if (item.ItemFlags.HasFlag(ItemFlags.Bless))
            {
                IAura blessAura = item.GetAura("Bless");
                if (!SavesDispel(level, blessAura?.Level ?? item.Level, 0))
                {
                    if (blessAura != null)
                        item.RemoveAura(blessAura, false);
                    caster.Act(ActOptions.ToAll, "{0} glows with a red aura.", item);
                    item.RemoveBaseItemFlags(ItemFlags.Bless);
                    return;
                }
                else
                    caster.Act(ActOptions.ToCharacter, "The holy aura of {0} is too powerful for you to overcome.");
                return;
            }
            AuraManager.AddAura(item, this, caster, level, TimeSpan.FromMinutes(2 * level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = 1, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = ItemFlags.Evil, Operator = AffectOperators.Or });
            caster.Act(ActOptions.ToAll, "{0} glows with a malevolent aura.", item);
        }
    }
}
