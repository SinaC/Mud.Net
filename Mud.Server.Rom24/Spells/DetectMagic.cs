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
[AbilityCharacterWearOffMessage("The detect magic wears off.")]
[AbilityDispellable]
[Syntax("cast [spell]")]
[Help(
@"This spell enables the caster to detect magical objects.")]
public class DetectMagic : CharacterFlagsSpellBase
{
    private const string SpellName = "Detect Magic";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }

    public DetectMagic(ILogger<DetectMagic> logger, IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
        CharacterFlagFactory = characterFlagFactory;
    }

    protected override ICharacterFlags CharacterFlags => CharacterFlagFactory.CreateInstance("DetectMagic");
    protected override string SelfAlreadyAffected => "You can already sense magical auras.";
    protected override string NotSelfAlreadyAffected => "{0:N} can already detect magic.";
    protected override string SelfSuccess => "Your eyes tingle.";
    protected override string NotSelfSuccess => "Ok.";
    protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
}
