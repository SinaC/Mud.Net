using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class AssistAction : IGameAction
{
    private readonly Mob _victim;
    private readonly Mob _attacker;

    public AssistAction(Mob victim, Mob attacker)
    {
        _victim = victim;
        _attacker = attacker;
    }

    public void Execute(World world)
    {
        var room = _victim.CurrentRoom;

        foreach (var mob in room.Mobs.ToList())
        {
            if (mob.IsDead || mob.InCombat)
                continue;

            if (ShouldAssist(mob, _victim))
            {
                mob.CurrentTarget = _attacker;
                mob.Position = Position.Fighting;
            }
        }
    }

    private bool ShouldAssist(Mob helper, Mob victim)
    {
        if (helper.IsCharmed && helper.Master == victim)
            return true;

        if (helper.Group == victim.Group)
            return true;

        return false;
    }
}
