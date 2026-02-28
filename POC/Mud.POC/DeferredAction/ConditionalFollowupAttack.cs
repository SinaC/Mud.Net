using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class ConditionalFollowupAttack : IGameAction
{
    private readonly Mob _attacker;

    public ConditionalFollowupAttack(Mob attacker)
    {
        _attacker = attacker;
    }

    public void Execute(World world)
    {
        if (!_attacker.CanAct) return;

        var victim = _attacker.CurrentTarget;

        if (victim == null || victim.IsDead)
            return;

        if (_attacker.CurrentRoom != victim.CurrentRoom)
        {
            _attacker.CurrentTarget = null; // ROM stop_fighting
            return;
        }

        world.Enqueue(new AttackAction(_attacker, victim));
    }
}
