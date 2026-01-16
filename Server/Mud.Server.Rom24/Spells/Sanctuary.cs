using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Random;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[AbilityCharacterWearOffMessage("The white aura around your body fades.")]
[AbilityDispellable("The white aura around {0:n}'s body vanishes.")]
[Syntax("cast [spell] <character>")]
[Help(
@"The SANCTUARY spell reduces the damage taken by the character from any attack
by one half.")]
[OneLineHelp("reduces all damage taken by the recipient by half")]
public class Sanctuary : CharacterBuffSpellBase
{
    private const string SpellName = "Sanctuary";

    public Sanctuary(ILogger<Sanctuary> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already in sanctuary.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already in sanctuary.";
    protected override string VictimAffectMessage => "You are surrounded by a %W%white%x% aura.";
    protected override string CasterAffectMessage => "{0:N} is surrounded by a %W%white%x% aura.";

    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(Level / 6),
        new IAffect[]
        {
            new CharacterShieldFlagsAffect{ Modifier = new ShieldFlags("Sanctuary"), Operator = AffectOperators.Or },
            new SanctuaryDamageModifierAffect()
        });
}
