using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public interface ISavingThrowRule
{
    bool CheckSave(CombatContext ctx);
}
