using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.POC.Affects;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff, CooldownInSeconds = 7)]
[AbilityCharacterWearOffMessage("%W%Power Word: Shield%x% fades away.")]
[Syntax("cast [spell] <character>")]
public class PowerWordShield : CharacterBuffSpellBase
{
    private const string SpellName = "Power Word: Shield";

    public PowerWordShield(ILogger<FireShield> logger, IRandomManager randomManager, IAuraManager auraManager)
    : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => $"You are already affected by %W%Power Word: Shield%x%";

    protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already affected by %W%Power Word: Shield%x%";

    protected override string VictimAffectMessage => "You are surrounded by %W%Power Word: Shield%x%";

    protected override string CasterAffectMessage => "{0:N} is surrounded by %W%Power Word: Shield%x%";

    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromSeconds(15),
        new IAffect[]
        {
            new AbsorbDamageAffect { RemainingAbsorb = Level * 10 }
        });
}
