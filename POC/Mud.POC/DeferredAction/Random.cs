using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class Random
{
    public static System.Random Shared { get; } = new System.Random();
}
