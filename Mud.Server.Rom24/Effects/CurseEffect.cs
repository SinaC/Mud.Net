using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Effects;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Rom24.Effects;

[Effect("Curse")]
public class CurseEffect : IEffect<ICharacter>
{
    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }
    private IAuraManager AuraManager { get; }

    public CurseEffect(IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IAuraManager auraManager)
    {
        CharacterFlagFactory = characterFlagFactory;
        AuraManager = auraManager;
    }

    public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
    {
        var curseAura = victim.GetAura(abilityName);
        if (curseAura != null || victim.CharacterFlags.IsSet("Curse") || victim.SavesSpell(level, SchoolTypes.Negative))
            return;
        victim.Send("You feel unclean.");
        if (victim != source && source is ICharacter sourceCharacter)
            sourceCharacter.Act(ActOptions.ToCharacter, "{0:N} looks very uncomfortable.", victim);
        int duration = 2 * level;
        int modifier = level / 8;
        AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -modifier, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = CharacterFlagFactory.CreateInstance("Curse"), Operator = AffectOperators.Or });
    }
}
