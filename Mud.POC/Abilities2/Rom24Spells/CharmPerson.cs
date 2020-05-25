using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CharmPerson : OffensiveSpellBase
    {
        public override int Id => 14;
        public override string Name => "Charm Person";
        public override AbilityEffects Effects => AbilityEffects.Animation;

        private IAuraManager AuraManager { get; }
        public CharmPerson(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.IsSafe(caster))
                return;

            if (caster == victim)
            {
                caster.Send("You like yourself even better!");
                return;
            }

            IPlayableCharacter pcCaster = caster as IPlayableCharacter;
            if (pcCaster == null)
            {
                caster.Send("You can't charm!");
                return;
            }

            INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
            if (npcVictim == null)
            {
                caster.Send("You can't charm players!");
                return;
            }

            if (npcVictim.CharacterFlags.HasFlag(CharacterFlags.Charm)
                || caster.CharacterFlags.HasFlag(CharacterFlags.Charm)
                || level < npcVictim.Level
                || npcVictim.Immunities.HasFlag(IRVFlags.Charm)
                || npcVictim.SavesSpell(level, SchoolTypes.Charm))
                return;

            if (npcVictim.Room.RoomFlags.HasFlag(RoomFlags.Law))
            {
                caster.Send("The mayor does not allow charming in the city limits.");
                return;
            }

            pcCaster.AddPet(npcVictim);

            int duration = RandomManager.Fuzzy(level / 4);
            AuraManager.AddAura(npcVictim, this, caster, level, TimeSpan.FromHours(duration), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Charm, Operator = AffectOperators.Or });

            npcVictim.Act(ActOptions.ToCharacter, "Isn't {0} just so nice?", caster);
            if (caster != npcVictim)
                caster.Act(ActOptions.ToCharacter, "{0:N} looks at you with adoring eyes.", npcVictim);
        }
    }
}
