using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Effects;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Rom24.Effects;

[Effect("Blindness")]
public class BlindnessEffect : IEffect<ICharacter>
{
    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }
    private IAuraManager AuraManager { get; }

    public BlindnessEffect(IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IAuraManager auraManager)
    {
        CharacterFlagFactory = characterFlagFactory;
        AuraManager = auraManager;
    }

    public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
    {
        if (victim.CharacterFlags.IsSet("Blind") || victim.GetAura(abilityName) != null || victim.SavesSpell(level, SchoolTypes.None))
            return;
        AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromMinutes(1 + level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = CharacterFlagFactory.CreateInstance("Blind"), Operator = AffectOperators.Add });
        victim.Send("You are blinded!");
        victim.Act(ActOptions.ToRoom, "{0:N} appears to be blinded.", victim);
    }
}
