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
[AbilityCharacterWearOffMessage("Your icy shield slowly melts away.")]
[Syntax("cast [spell] <character>")]
public class IceShield : ShieldFlagsSpellBase
{
    private const string SpellName = "Ice Shield";

    private IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }

    public IceShield(ILogger<IceShield> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory)
        : base(logger, randomManager, auraManager)
    {
        ShieldFlagFactory = shieldFlagFactory;
    }

    protected override IShieldFlags ShieldFlags => ShieldFlagFactory.CreateInstance("IceShield");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level / 6);
    protected override string SelfAlreadyAffected => "You are already surrounded by a %C%icy%x% shield.";
    protected override string NotSelfAlreadyAffected => "{0:N} {0:b} already surrounded by a %C%icy%x% shield.";
    protected override string NotSelfSuccess => "You are surrounded by a %C%icy%x% shield.";
    protected override string SelfSuccess => "{0:N} is surrounded by a %C%icy%x% shield.";
}
