using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.POC.Affects;
using Mud.Random;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("Your crackling shield sizzles and fades.")]
[Syntax("cast [spell] <character>")]
public class ShockShield : CharacterBuffSpellBase
{
    private const string SpellName = "Shock Shield";

    public ShockShield(ILogger<FireShield> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already surrounded in a %Y%crackling%x% shield.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already surrounded by a %Y%crackling%x% shield.";
    protected override string VictimAffectMessage => "You are surrounded by a %Y%crackling%x% shield.";
    protected override string CasterAffectMessage => "{0:N} is surrounded by a %Y%crackling%x% shield.";

    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(Level / 6),
        new IAffect[]
        {
            new CharacterShieldFlagsAffect{ Modifier = new ShieldFlags("ShockShield"), Operator = AffectOperators.Or },
            new ShockShieldAfterHitAffect(RandomManager)
        });
}
