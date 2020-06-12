using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Debuff | AbilityEffects.Animation)]
    public class CharmPerson : OffensiveSpellBase
    {
        public const string SpellName = "Charm Person";

        private IAuraManager AuraManager { get; }

        public CharmPerson(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        public override string Setup(ISpellActionInput spellActionInput)
        {
            string baseSetup = base.Setup(spellActionInput);
            if (baseSetup != null)
                return baseSetup;
            IPlayableCharacter pcCaster = Caster as IPlayableCharacter;
            if (pcCaster == null)
                return "You can't charm!";
            return null;
        }

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            string baseSetTargets = base.SetTargets(spellActionInput);
            if (baseSetTargets != null)
                return baseSetTargets;

            if (Victim.IsSafe(Caster))
                return "Not on that victim.";

            if (Caster == Victim)
                return "You like yourself even better!";

            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            if (npcVictim == null)
                return "You can't charm players!";

            if (npcVictim.Room.RoomFlags.HasFlag(RoomFlags.Law))
                return "The mayor does not allow charming in the city limits.";

            return null;
        }

        protected override void Invoke()
        {
            INonPlayableCharacter npcVictim = (INonPlayableCharacter)Victim; // SetTargets ensure this will never failed
            if (npcVictim.CharacterFlags.HasFlag(CharacterFlags.Charm)
                || Caster.CharacterFlags.HasFlag(CharacterFlags.Charm)
                || Level < npcVictim.Level
                || npcVictim.Immunities.HasFlag(IRVFlags.Charm)
                || npcVictim.SavesSpell(Level, SchoolTypes.Charm))
                return;

            ((IPlayableCharacter)Caster).AddPet(npcVictim); // Guards ensure this will never failed

            int duration = RandomManager.Fuzzy(Level / 4);
            AuraManager.AddAura(npcVictim, SpellName, Caster, Level, TimeSpan.FromHours(duration), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Charm, Operator = AffectOperators.Or });

            npcVictim.Act(ActOptions.ToCharacter, "Isn't {0} just so nice?", Caster);
            if (Caster != npcVictim)
                Caster.Act(ActOptions.ToCharacter, "{0:N} looks at you with adoring eyes.", npcVictim);
        }
    }
}
