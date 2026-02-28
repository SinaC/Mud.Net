using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

[Flags]
public enum RoomFlags
{
    None = 0,
    PkAllowed = 1 << 0,
    Safe = 1 << 1
}
