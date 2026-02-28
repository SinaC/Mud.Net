using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Detection;

public interface IDetectionRule
{
    void Apply(DetectionContext ctx);
}
