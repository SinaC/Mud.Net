using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
[AbilityCharacterWearOffMessage("The detect magic wears off.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to detect magical objects.")]
[OneLineHelp("reveals magical auras to the caster")]
public class DetectMagic : CharacterFlagsSpellBase
{
    private const string SpellName = "Detect Magic";

    public DetectMagic(ILogger<DetectMagic> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override ICharacterFlags CharacterFlags => new CharacterFlags("DetectMagic");
    protected override string SelfAlreadyAffected => "You can already sense magical auras.";
    protected override string NotSelfAlreadyAffected => "{0:N} can already detect magic.";
    protected override string SelfSuccess => "Your eyes tingle.";
    protected override string NotSelfSuccess => "Ok.";
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
}
