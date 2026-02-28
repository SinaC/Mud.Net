using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

[Flags]
public enum NpcFlags
{
    None = 0,
    Aggressive = 1 << 0,
    Scavenger = 1 << 1,
    Sentinel = 1 << 2,
    Memory = 1 << 3,
    Guard = 1 << 4,
}
