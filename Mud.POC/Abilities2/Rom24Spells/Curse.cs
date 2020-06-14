using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;
using Mud.POC.Abilities2.Rom24Effects;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("The curse wears off.")]
    [AbilityItemWearOffMessage("{0} is no longer impure.")]
    [AbilityDispellable("{0} is no longer impure.")]
    public class Curse : ItemOrOffensiveSpellBase
    {
        public const string SpellName = "Curse";

        private IAuraManager AuraManager { get; }
        private IDispelManager DispelManager { get; }

        public Curse(IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            DispelManager = dispelManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            CurseEffect effect = new CurseEffect(AuraManager);
            effect.Apply(victim, Caster, SpellName, Level, 0);
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
                IAura blessAura = item.GetAura(Bless.SpellName);
                if (!DispelManager.SavesDispel(Level, blessAura?.Level ?? item.Level, 0))
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
            AuraManager.AddAura(item, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(2 * Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = 1, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = ItemFlags.Evil, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0} glows with a malevolent aura.", item);
        }
    }
}
