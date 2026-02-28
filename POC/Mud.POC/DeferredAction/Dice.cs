using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public static class Dice
{
    internal static int Roll(int level, int v)
    {
        return level * v / 2;
        // TODO
    }
}
