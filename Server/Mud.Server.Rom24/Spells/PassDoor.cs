using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel solid again.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to pass through closed doors.")]
[OneLineHelp("allows the caster to walk through walls")]
public class PassDoor : CharacterFlagsSpellBase
{
    private const string SpellName = "Pass Door";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    public PassDoor(ILogger<PassDoor> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override ICharacterFlags CharacterFlags => new CharacterFlags("PassDoor");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(RandomManager.Fuzzy(Level / 4));
    protected override string SelfAlreadyAffected => "You are already out of phase.";
    protected override string NotSelfAlreadyAffected => "{0:N} is already shifted out of phase.";
    protected override string SelfSuccess => "You turn translucent.";
    protected override string NotSelfSuccess => "{0} turns translucent.";
}
