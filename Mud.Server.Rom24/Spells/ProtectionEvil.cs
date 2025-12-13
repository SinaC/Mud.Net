using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
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
[OneLineHelp("provides defense from the attacks of evil creatures")]
public class ProtectionEvil : CharacterBuffSpellBase
{
    private const string SpellName = "Protection Evil";

    private IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }

    public ProtectionEvil(ILogger<ProtectionEvil> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory)
        : base(logger, randomManager, auraManager)
    {
        ShieldFlagFactory = shieldFlagFactory;
    }

    protected override string SelfAlreadyAffectedMessage => "You are already protected.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already protected.";
    protected override string VictimAffectMessage => "You feel holy and pure.";
    protected override string CasterAffectMessage => "{0:N} is protected from evil.";
    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(24),
        new IAffect[] 
        {
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
            new CharacterShieldFlagsAffect(ShieldFlagFactory) { Modifier = ShieldFlagFactory.CreateInstance("ProtectEvil"), Operator = AffectOperators.Or }
        });
    
}
