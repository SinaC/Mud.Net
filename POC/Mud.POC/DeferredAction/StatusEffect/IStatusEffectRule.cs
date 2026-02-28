using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public interface IStatusEffectRule
{
    void OnApply(Mob target, World world);
    void OnTick(Mob target, World world);
    void OnRemove(Mob target, World world);

    void ModifyCombat(CombatContext context);
    void ModifyDetection(DetectionContext context);
}
