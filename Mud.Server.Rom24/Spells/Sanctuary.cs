using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("The white aura around your body fades.")]
[AbilityDispellable("The white aura around {0:n}'s body vanishes.")]
[Syntax("cast [spell] <character>")]
[Help(
@"The SANCTUARY spell reduces the damage taken by the character from any attack
by one half.")]
public class Sanctuary : ShieldFlagsSpellBase
{
    private const string SpellName = "Sanctuary";

    private IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }

    public Sanctuary(ILogger<Sanctuary> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory)
        : base(logger, randomManager, auraManager)
    {
        ShieldFlagFactory = shieldFlagFactory;
    }

    protected override IShieldFlags ShieldFlags => ShieldFlagFactory.CreateInstance("Sanctuary");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level / 6);
    protected override string SelfAlreadyAffected => "You are already in sanctuary.";
    protected override string NotSelfAlreadyAffected => "{0:N} is already in sanctuary.";
    protected override string SelfSuccess => "You are surrounded by a white aura.";
    protected override string NotSelfSuccess => "{0:N} is surrounded by a white aura.";
}
