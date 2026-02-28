using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class FleeCommand : PlayerCommand
{
    public override void Execute(Mob player, World world)
    {
        if (!player.InCombat)
            return;

        world.Enqueue(new FleeAction(player));
    }
}
