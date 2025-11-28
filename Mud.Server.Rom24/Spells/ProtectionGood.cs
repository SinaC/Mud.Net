using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel less protected.")]
[AbilityDispellable]
[Syntax("cast [spell] <target>")]
[Help(
@"The protection spells reduce damage taken from attackers of the appropriate
ethos, and improve saving throws against all forms of magic. They may not
be cast on others, and one person cannot carry both defenses at the same
time.")]
public class ProtectionGood : CharacterBuffSpellBase
{
    private const string SpellName = "Protection Good";

    private IServiceProvider ServiceProvider { get; }

    public ProtectionGood(ILogger<ProtectionGood> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        ServiceProvider = serviceProvider;
    }

    protected override string SelfAlreadyAffectedMessage => "You are already protected.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already protected.";
    protected override string VictimAffectMessage => "You feel aligned with darkness.";
    protected override string CasterAffectMessage => "{0:N} is protected from good.";
    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(24),
        new IAffect[] 
        {
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "ProtectGood"), Operator = AffectOperators.Or }
        });
    
}
