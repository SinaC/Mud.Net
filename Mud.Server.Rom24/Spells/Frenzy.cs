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

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24)]
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
public class Frenzy : DefensiveSpellBase
{
    private const string SpellName = "Frenzy";

    private IAuraManager AuraManager { get; }

    public Frenzy(ILogger<Frenzy> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        FrenzyEffect effect = new (AuraManager);
        effect.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
