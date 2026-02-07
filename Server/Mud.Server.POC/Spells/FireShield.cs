using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.POC.Affects;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("Your firey shield gutters out.")]
[Syntax("cast [spell] <character>")]
public class FireShield : CharacterBuffSpellBase
{
    private const string SpellName = "Fire Shield";

    public FireShield(ILogger<FireShield> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already surrounded by a %R%firey%x% shield.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already surrounded by a %R%firey%x% shield.";
    protected override string VictimAffectMessage => "You are surrounded by a %R%fiery%x% shield.";
    protected override string CasterAffectMessage => "{0:N} is surrounded by a %R%fiery%x% shield.";

    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(Level / 6),
        new IAffect[]
        {
            new CharacterShieldFlagsAffect { Modifier = new ShieldFlags("FireShield"), Operator = AffectOperators.Or },
            new FireShieldAfterHitAffect(RandomManager)
        });
}
