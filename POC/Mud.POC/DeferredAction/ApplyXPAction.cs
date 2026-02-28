using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class ApplyXPAction : IGameAction
{
    private readonly Mob _mob;
    private readonly int _xp;

    public ApplyXPAction(Mob mob, int xp)
    {
        _mob = mob;
        _xp = xp;
    }

    public void Execute(World world)
    {
        if (_mob.IsDead) return;

        _mob.Experience += _xp;

        // Optional: level up check
        if (_mob.Experience >= _mob.NextLevelXP)
        {
            world.Enqueue(new LevelUpAction(_mob));
        }
    }
}
