using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[AbilityCharacterWearOffMessage("Your rage ebbs.")]
[AbilityDispellable("{0:N} no longer looks so wild.")]
[Syntax("cast [spell] <target>")]
[Help(
@"The frenzy spell fills the target with righteous fury, greatly increasing
his or her attack skill and damaging capacity.  Unfortunately, this divine
wrath is coupled with a tendency to ignore threats to personal safety, 
making the character easier to hit.  Frenzy provides immunity to the calm
spell (see 'help calm'), and may only be used on those of the caster's 
alignment.")]
[OneLineHelp("puts the recipient into beserker rage")]
public class Frenzy : DefensiveSpellBase
{
    private const string SpellName = "Frenzy";

    private IEffectManager EffectManager { get; }

    public Frenzy(ILogger<Frenzy> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        var effect = EffectManager.CreateInstance<ICharacter>("Frenzy");
        effect?.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
