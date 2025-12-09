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
[AbilityCharacterWearOffMessage("You feel less aware of your surroundings.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to detect hidden creatures.")]
[OneLineHelp("shows the whereabouts of hidden foes")]
public class DetectHidden : CharacterFlagsSpellBase
{
    private const string SpellName = "Detect Hidden";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }

    public DetectHidden(ILogger<DetectHidden> logger, IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        CharacterFlagFactory = characterFlagFactory;
    }

    protected override ICharacterFlags CharacterFlags => CharacterFlagFactory.CreateInstance("DetectHidden");
    protected override string SelfAlreadyAffected => "You are already as alert as you can be.";
    protected override string NotSelfAlreadyAffected => "{0:N} can already sense hidden lifeforms.";
    protected override string SelfSuccess => "Your awareness improves.";
    protected override string NotSelfSuccess => "Ok.";
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
}
