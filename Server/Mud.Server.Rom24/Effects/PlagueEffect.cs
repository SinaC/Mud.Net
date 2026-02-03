using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Rom24.Effects;

[Effect("Plague")]
public class PlagueEffect : IEffect<ICharacter>
{
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }
    private IAffectManager AffectManager { get; }

    public PlagueEffect(IRandomManager randomManager, IAuraManager auraManager, IAffectManager affectManager)
    {
        RandomManager = randomManager;
        AuraManager = auraManager;
        AffectManager = affectManager;
    }

    public void Apply(ICharacter victim, IEntity source, string auraName, int level, int modifier)
    {
        if (!victim.IsValid)
            return;

        if (victim.CharacterFlags.IsSet("Plague"))
            return;

        if (victim.SavesSpell(level, SchoolTypes.Disease)
            || (victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.IsSet("Undead")))
            return;

        var plagueAffect = AffectManager.CreateInstance("Plague");
        victim.Send("You feel hot and feverish.");
        victim.Act(ActOptions.ToRoom, "{0:N} shivers and looks very ill.", victim);
        var duration = RandomManager.Range(1, 2 * level);
        AuraManager.AddAura(victim, auraName, victim, level, TimeSpan.FromMinutes(duration), new AuraFlags(), true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = modifier, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Plague"), Operator = AffectOperators.Or },
            plagueAffect,
            new CharacterRegenModifierAffect { Modifier = 8, Operator = AffectOperators.Divide });
    }
}
