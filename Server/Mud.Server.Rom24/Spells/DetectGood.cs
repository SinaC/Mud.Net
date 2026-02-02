using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
[AbilityCharacterWearOffMessage("The gold in your vision disappears.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to detect good characters, which will
reveal a characteristic golden aura.")]
[OneLineHelp("reveals the aura of good monsters")]
public class DetectGood : CharacterFlagsSpellBase
{
    private const string SpellName = "Detect Good";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    public DetectGood(ILogger<DetectGood> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override ICharacterFlags CharacterFlags => new CharacterFlags("DetectGood");
    protected override string SelfAlreadyAffected => "You can already sense good.";
    protected override string NotSelfAlreadyAffected => "{0:N} can already detect good.";
    protected override string SelfSuccess => "Your eyes tingle.";
    protected override string NotSelfSuccess => "Ok.";
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
}
