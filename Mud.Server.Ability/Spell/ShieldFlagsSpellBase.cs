using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class ShieldFlagsSpellBase : DefensiveSpellBase
{
    protected IAuraManager AuraManager { get; }

    protected ShieldFlagsSpellBase(ILogger<ShieldFlagsSpellBase> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.ShieldFlags.HasAny(ShieldFlags))
        {
            if (Victim == Caster)
                Caster.Send(SelfAlreadyAffected);
            else
                Caster.Act(ActOptions.ToCharacter, NotSelfAlreadyAffected, Victim);
            return;
        }
        var duration = Duration;
        AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, duration, AuraFlags.None, true,
            new CharacterShieldFlagsAffect { Modifier = ShieldFlags, Operator = AffectOperators.Or });
        Victim.Send(SelfSuccess);
        if (Victim != Caster)
            Victim.Act(ActOptions.ToRoom, NotSelfSuccess, Victim);
    }

    protected abstract IShieldFlags ShieldFlags { get; }
    protected abstract TimeSpan Duration { get; }
    protected abstract string SelfAlreadyAffected { get; }
    protected abstract string NotSelfAlreadyAffected { get; }
    protected abstract string SelfSuccess { get; }
    protected abstract string NotSelfSuccess { get; }
}
