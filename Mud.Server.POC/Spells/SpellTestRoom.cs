using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Room;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
public class SpellTestRoom : CharacterBuffSpellBase
{
    private const string SpellName = "Testroom";

    public SpellTestRoom(ILogger<SpellTestRoom> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already affected.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already affected.";
    protected override string VictimAffectMessage => "You are now affected by Testroom.";
    protected override string CasterAffectMessage => "{0:N} {0:b} now affected by Testroom.";

    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo => (Caster.Level, TimeSpan.FromMinutes(5),
        new IAffect[]
        {
            new RoomHealRateAffect {Modifier = 10, Operator = AffectOperators.Add},
            new RoomResourceRateAffect {Modifier = 150, Operator = AffectOperators.Assign},
            new RoomFlagsAffect {Modifier = new RoomFlags("NoScan", "NoWhere"), Operator = AffectOperators.Or}
        });

    protected override void Invoke()
    {
        base.Invoke();

        Victim.Room.Recompute();
    }
}
