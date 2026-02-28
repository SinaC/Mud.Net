using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Detection;

public class DetectionContext
{
    public Mob Detector { get; }
    public Mob Target { get; }

    public int DetectionChance { get; set; }

    public bool Detected { get; set; }

    public DetectionContext(Mob detector, Mob target)
    {
        Detector = detector;
        Target = target;
    }
}
