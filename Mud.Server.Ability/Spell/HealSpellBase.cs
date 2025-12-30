using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class HealSpellBase : DefensiveSpellBase
{
    protected HealSpellBase(ILogger<HealSpellBase> logger, IRandomManager randomManager) 
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        int value = HealValue;
        Victim.UpdateResource(ResourceKinds.HitPoints, value);
        Victim.Send(HealVictimPhrase);
        if (Caster != Victim)
            Caster.Send(HealCasterPhrase);
    }

    protected abstract int HealValue { get; }
    protected abstract string HealVictimPhrase { get; }
    protected virtual string HealCasterPhrase => "Ok.";
}
