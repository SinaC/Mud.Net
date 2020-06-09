using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Spells;

namespace Mud.POC.Abilities2.Rom24Effects
{
    public class FrenzyEffect : IEffect<ICharacter>
    {
        private IAuraManager AuraManager { get; }

        public FrenzyEffect(IAuraManager auraManager)
        {
            AuraManager = auraManager;
        }

        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.GetAura(abilityName) != null)
            {
                if (victim == source)
                    source.Send("You are already in a frenzy.");
                else
                    source.Act(ActOptions.ToCharacter, "{0:N} is already in a frenzy.", victim);
                return;
            }

            if (victim.CharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura(Calm.SpellName) != null)
            {
                if (victim == source)
                    source.Send("Why don't you just relax for a while?");
                else
                    source.Act(ActOptions.ToCharacter, "{0:N} doesn't look like $e wants to fight anymore.", victim);
                return;
            }

            if (source is ICharacter characterSource)
            {
                if ((characterSource.IsGood && !victim.IsGood)
                    || (characterSource.IsNeutral && !victim.IsNeutral)
                    || (characterSource.IsEvil && !victim.IsEvil))
                {
                    source.Act(ActOptions.ToCharacter, "Your god doesn't seem to like {0:N}.", victim);
                    return;
                }
            }

            int duration = level / 3;
            int modifier = level / 6;
            AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = (10 * level) / 12, Operator = AffectOperators.Add });

            victim.Send("You are filled with holy wrath!");
            victim.Act(ActOptions.ToRoom, "{0:N} gets a wild look in $s eyes!", victim);
        }
    }
}
