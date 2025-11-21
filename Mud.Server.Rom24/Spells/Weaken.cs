using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You feel stronger.")]
[AbilityDispellable("{0:N} looks stronger.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell reduces the strength of the victim by two points.")]
public class Weaken : CharacterDebuffSpellBase
{
    private const string SpellName = "Weaken";

    private IServiceProvider ServiceProvider { get; }

    public Weaken(ILogger<Weaken> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        ServiceProvider = serviceProvider;
    }

    protected override SchoolTypes DebuffType => SchoolTypes.Other;
    protected override string VictimAffectMessage => "You feel your strength slip away.";
    protected override string RoomAffectMessage => "{0:N} looks tired and weak.";
    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(Level / 2),
        new IAffect[]
        {
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -Level/5, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Weaken"), Operator = AffectOperators.Or }
        });

    protected override bool CanAffect => base.CanAffect && !Victim.CharacterFlags.IsSet("Weaken");
}
