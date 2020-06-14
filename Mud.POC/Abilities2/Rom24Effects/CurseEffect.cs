using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2.Rom24Effects
{
    public class CurseEffect : IEffect<ICharacter>
    {
        private IAuraManager AuraManager { get; }

        public CurseEffect(IAuraManager auraManager)
        {
            AuraManager = auraManager;
        }

        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            IAura curseAura = victim.GetAura(abilityName);
            if (curseAura != null || victim.CharacterFlags.HasFlag(CharacterFlags.Curse) || victim.SavesSpell(level, SchoolTypes.Negative))
                return;
            victim.Send("You feel unclean.");
            if (source != victim)
                source.Act(ActOptions.ToCharacter, "{0:N} looks very uncomfortable.", victim);
            int duration = 2 * level;
            int modifier = level / 8;
            AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Curse, Operator = AffectOperators.Or });
        }
    }
}
