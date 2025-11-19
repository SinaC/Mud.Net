using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
public class RayOfTruth : OffensiveSpellBase
{
    private const string SpellName = "Ray of Truth";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public RayOfTruth(ILogger<RayOfTruth> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
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

        var damageResult = Victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "ray of truth", true);
        if (damageResult == DamageResults.Done)
        {
            BlindnessEffect effect = new (ServiceProvider, AuraManager);
            effect.Apply(Victim, Caster, "Blindness", 3*Level/4, 0);
        }
    }
}
