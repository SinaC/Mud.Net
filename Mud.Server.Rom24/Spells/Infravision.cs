using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
[AbilityCharacterWearOffMessage("You no longer see in the dark.")]
[AbilityDispellable]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell enables the target character to see warm-blooded creatures even
while in the dark, and exits of a room as well.")]
[OneLineHelp("allows monsters to be seen in the dark")]
public class Infravision : CharacterFlagsSpellBase
{
    private const string SpellName = "Infravision";

    public Infravision(ILogger<Infravision> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override ICharacterFlags CharacterFlags => new CharacterFlags("Infrared");
    protected override TimeSpan Duration => TimeSpan.FromMinutes(2*Level);
    protected override string SelfAlreadyAffected => "You can already see in the dark.";
    protected override string NotSelfAlreadyAffected => "{0} already has infravision.";
    protected override string SelfSuccess => "Your eyes glow red.";
    protected override string NotSelfSuccess => "{0:P} eyes glow red.";
}
