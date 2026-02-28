using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public static class CombatFormulas
{
    public static int RollDamage(Mob attacker, Mob target)
        => 1;

    public static int CalculateXP(Mob victim)
        => 1;

    public static int RollHitPoints(int level)
        => 1;
}
