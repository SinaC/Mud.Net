using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using System;

namespace Mud.Server.Rom24.Effects
{
    public class BlessEffect : IEffect<ICharacter>
    {
        private IAuraManager AuraManager { get; }

        public BlessEffect(IAuraManager auraManager)
        {
            AuraManager = auraManager;
        }

        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            IAura blessAura = victim.GetAura(abilityName);
            if (blessAura != null) // test on fighting removed
            {
                if (source == victim)
                    source.Send("You are already blessed.");
                else
                    victim.Act(ActOptions.ToCharacter, "{0:N} already has divine favor.", victim);
                return;
            }
            victim.Send("You feel righteous.");
            if (victim != source && source is ICharacter sourceCharacter)
                sourceCharacter.Act(ActOptions.ToCharacter, "You grant {0} the favor of your god.", victim);
            int duration = 6 + level;
            AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = level / 8, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -level / 8, Operator = AffectOperators.Add });
        }
    }
}
