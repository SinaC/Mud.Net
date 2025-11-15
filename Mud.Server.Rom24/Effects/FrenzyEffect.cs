using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Effects;

public class FrenzyEffect : IEffect<ICharacter>
{
    private IAuraManager AuraManager { get; }

    public FrenzyEffect(IAuraManager auraManager)
    {
        AuraManager = auraManager;
    }

    public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
    {
        var sourceCharacter = source as ICharacter;
        if (victim.CharacterFlags.IsSet("Berserk") || victim.GetAura(abilityName) != null)
        {
            if (victim == source)
                source.Send("You are already in a frenzy.");
            else 
                sourceCharacter?.Act(ActOptions.ToCharacter, "{0:N} is already in a frenzy.", victim);
            return;
        }

        if (victim.CharacterFlags.IsSet("Calm") || victim.GetAura("Calm") != null)
        {
            if (victim == source)
                source.Send("Why don't you just relax for a while?");
            else
                sourceCharacter?.Act(ActOptions.ToCharacter, "{0:N} doesn't look like $e wants to fight anymore.", victim);
            return;
        }

        if (sourceCharacter is not null)
        {
            if ((sourceCharacter.IsGood && !victim.IsGood)
                || (sourceCharacter.IsNeutral && !victim.IsNeutral)
                || (sourceCharacter.IsEvil && !victim.IsEvil))
            {
                sourceCharacter.Act(ActOptions.ToCharacter, "Your god doesn't seem to like {0:N}.", victim);
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
