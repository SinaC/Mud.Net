using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
[AbilityCharacterWearOffMessage("The red in your vision disappears.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to detect evil characters, which will
reveal a characteristic red aura.")]
public class DetectEvil : CharacterFlagsSpellBase
{
    private const string SpellName = "Detect Evil";

    private IServiceProvider ServiceProvider { get; }

    public DetectEvil(ILogger<DetectEvil> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        ServiceProvider = serviceProvider;
    }

    protected override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "DetectEvil");
    protected override string SelfAlreadyAffected => "You can already sense evil.";
    protected override string NotSelfAlreadyAffected => "{0:N} can already detect evil.";
    protected override string SelfSuccess => "Your eyes tingle.";
    protected override string NotSelfSuccess => "Ok.";
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
}
