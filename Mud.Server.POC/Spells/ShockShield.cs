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
[AbilityCharacterWearOffMessage("Your crackling shield sizzles and fades.")]
[Syntax("cast [spell] <character>")]
public class ShockShield : ShieldFlagsSpellBase
{
    private const string SpellName = "Shock Shield";

    public ShockShield(ILogger<FireShield> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory)
        : base(logger, randomManager, auraManager, shieldFlagFactory)
    {
    }

    protected override IShieldFlags ShieldFlags => FlagFactory.CreateInstance("ShockShield");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level / 6);
    protected override string SelfAlreadyAffected => "You are already surrounded in a %B%crackling%x% shield.";
    protected override string NotSelfAlreadyAffected => "{0:N} {0:b} already surrounded by a %B%crackling%x% shield.";
    protected override string SelfSuccess => "You are surrounded by a %B%crackling%x% shield.";
    protected override string NotSelfSuccess => "{0:N} is surrounded by a %B%crackling%x% shield.";
}
