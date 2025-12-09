using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.HealingArea, PulseWaitTime = 36)]
[Syntax("cast [spell]")]
[Help(
@"The mass healing spell, as its name might suggest, performs a healing spell
on all players in the room.  It also throws in a refresh spell for good
measure.")]
[OneLineHelp("casts a heal spell on each player in the room")]
public class MassHealing : NoTargetSpellBase
{
    private const string SpellName = "Mass Healing";

    private IEffectManager EffectManager { get; }

    public MassHealing(ILogger<MassHealing> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        foreach (var victim in Caster.Room.People)
        {
            if ((Caster is IPlayableCharacter && victim is IPlayableCharacter)
                || (Caster is INonPlayableCharacter && victim is INonPlayableCharacter))
            {
                var healEffect = EffectManager.CreateInstance<ICharacter>("Heal");
                healEffect?.Apply(victim, Caster, "Heal", Level, 0);
                var refreshEffect = EffectManager.CreateInstance<ICharacter>("Refresh");
                refreshEffect?.Apply(victim, Caster, "Refresh", Level, 0);
            }
        }
    }
}
