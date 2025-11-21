using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You can see again.")]
[AbilityDispellable("{0:N} is no longer blinded.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell renders the target character blind.")]
public class Blindness : OffensiveSpellBase
{
    private const string SpellName = "Blindness";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public Blindness(ILogger<Blindness> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        BlindnessEffect effect = new (ServiceProvider, AuraManager);
        effect.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
