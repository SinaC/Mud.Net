using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

[Flags]
public enum PlayerFlags
{
    None = 0,
    Killer = 1 << 0,
    Thief = 1 << 1
}
