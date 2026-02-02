using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <target>")]
[Help(
@"Ray of truth opens a portal to the planes of positive energy, bringing forth
a beam of light of sufficient purity to harm or or annihilate the servants
of evil.  It cannot harm the pure of heart, and will turn and strike 
casters who are tainted by evil.")]
[OneLineHelp("sends forth a blinding ray of holy energy")]
public class RayOfTruth : OffensiveSpellBase
{
    private const string SpellName = "Ray of Truth";

    private IEffectManager EffectManager { get; }

    public RayOfTruth(ILogger<RayOfTruth> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        var baseSetTargets = base.SetTargets(spellActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        // Check alignment
        if (Caster is IPlayableCharacter && !Caster.IsGood)
        {
            Victim = Caster;
            Caster.Send("The energy explodes inside you!");
        }
        return null;
    }


    protected override void Invoke()
    {
        if (Victim != Caster)
            Caster.Act(ActOptions.ToAll, "{0:N} raise{0:v} {0:s} hand, and a blinding ray of light shoots forth!", Caster);

        if (Victim.IsGood)
        {
            Victim.Act(ActOptions.ToRoom, "{0:N} seems unharmed by the light.", Victim);
            Victim.Send("The light seems powerless to affect you.");
            return;
        }

        int damage = RandomManager.Dice(Level, 10);
        if (Victim.SavesSpell(Level, SchoolTypes.Holy))
            damage /= 2;

        int alignment = Victim.Alignment - 350;
        if (alignment < -1000)
            alignment = -1000 + (alignment + 1000) / 3;

        damage = (damage * alignment * alignment) / (1000 * 1000);

        var damageResult = Victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, SpellName, true);
        if (damageResult == DamageResults.Done)
        {
            var blindnessEffect = EffectManager.CreateInstance<ICharacter>("Blindness");
            blindnessEffect?.Apply(Victim, Caster, "Blindness", 3*Level/4, 0);
        }
    }
}
