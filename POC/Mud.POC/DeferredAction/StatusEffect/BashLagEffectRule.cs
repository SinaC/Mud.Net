using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class BashLagEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        target.Wait = 2;
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world) { }

    public void ModifyCombat(CombatContext context) { }

    public void ModifyDetection(DetectionContext context) { }
}
