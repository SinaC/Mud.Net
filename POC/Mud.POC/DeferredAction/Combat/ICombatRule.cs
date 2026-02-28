using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public interface ICombatRule
{
    void Apply(CombatContext context);
}
