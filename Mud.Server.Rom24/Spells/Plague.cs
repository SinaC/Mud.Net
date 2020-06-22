using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Affects;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Debuff | AbilityEffects.Damage)]
    [AbilityCharacterWearOffMessage("Your sores vanish.")]
    public class Plague : OffensiveSpellBase
    {
        public const string SpellName = "Plague";

        private IAuraManager AuraManager { get; }

        public Plague(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.SavesSpell(Level, SchoolTypes.Disease)
                || (Victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
            {
                if (Victim == Caster)
                    Caster.Send("You feel momentarily ill, but it passes.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} seems to be unaffected.", Victim);
                return;
            }

            AuraManager.AddAura(Victim, SpellName, Caster, (3 * Level) / 4, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Plague, Operator = AffectOperators.Or },
                new PlagueSpreadAndDamageAffect(RandomManager, AuraManager));
            Victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", Victim);
        }
    }
}
