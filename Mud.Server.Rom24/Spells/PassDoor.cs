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
[AbilityCharacterWearOffMessage("You feel solid again.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to pass through closed doors.")]
public class PassDoor : CharacterFlagsSpellBase
{
    private const string SpellName = "Pass Door";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }

    public PassDoor(ILogger<PassDoor> logger, IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        CharacterFlagFactory = characterFlagFactory;
    }

    protected override ICharacterFlags CharacterFlags => CharacterFlagFactory.CreateInstance("PassDoor");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(RandomManager.Fuzzy(Level / 4));
    protected override string SelfAlreadyAffected => "You are already out of phase.";
    protected override string NotSelfAlreadyAffected => "{0:N} is already shifted out of phase.";
    protected override string SelfSuccess => "You turn translucent.";
    protected override string NotSelfSuccess => "{0} turns translucent.";
}
