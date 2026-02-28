using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class SingleHitAction : IGameAction
{
    private readonly Mob _attacker;
    private readonly Mob _victim;
    private readonly bool _isOffhand;

    public SingleHitAction(Mob attacker, Mob victim, bool isOffhand)
    {
        _attacker = attacker;
        _victim = victim;
        _isOffhand = isOffhand;
    }

    public void Execute(World world)
    {
        if (!_attacker.CanAct) return;
        if (_victim.IsDead) return;
        if (_attacker.CurrentRoom != _victim.CurrentRoom) return;

        world.Enqueue(new AttackAction(_attacker, _victim));
    }
}