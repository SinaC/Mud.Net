using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("Your firey shield gutters out.")]
[Syntax("cast [spell] <character>")]
public class FireShield : ShieldFlagsSpellBase
{
    private const string SpellName = "Fire Shield";

    private IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }

    public FireShield(ILogger<FireShield> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory)
        : base(logger, randomManager, auraManager)
    {
        ShieldFlagFactory = shieldFlagFactory;
    }

    protected override IShieldFlags ShieldFlags => ShieldFlagFactory.CreateInstance("FireShield");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level / 6);
    protected override string SelfAlreadyAffected => "You are already surrounded by a %R%firey%x% shield.";
    protected override string NotSelfAlreadyAffected => "{0:N} {0:b} already surrounded by a %R%firey%x% shield.";
    protected override string SelfSuccess => "You are surrounded by a %R%fiery%x% shield.";
    protected override string NotSelfSuccess => "{0:N} is surrounded by a %R%fiery%x% shield.";
}
