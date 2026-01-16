using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
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
