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

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
[AbilityCharacterWearOffMessage("You no longer see invisible objects.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to detect invisible objects and characters.")]
[OneLineHelp("allows the caster to see the unseeable")]
public class DetectInvis : CharacterFlagsSpellBase
{
    private const string SpellName = "Detect Invis";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }

    public DetectInvis(ILogger<DetectInvis> logger, IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        CharacterFlagFactory = characterFlagFactory;
    }

    protected override ICharacterFlags CharacterFlags => CharacterFlagFactory.CreateInstance("DetectInvis");
    protected override string SelfAlreadyAffected => "You can already see invisible.";
    protected override string NotSelfAlreadyAffected => "{0:N} can already see invisible things.";
    protected override string SelfSuccess => "Your eyes tingle.";
    protected override string NotSelfSuccess => "Ok.";
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
}
