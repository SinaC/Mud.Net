using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Class;

[Export(typeof(ISanityCheck)), Shared]
public class ClassSanityCheck : ISanityCheck
{
    private ILogger<ClassSanityCheck> Logger { get; }
    private IClassManager ClassManager { get; }

    public ClassSanityCheck(ILogger<ClassSanityCheck> logger, IClassManager classManager)
    {
        Logger = logger;
        ClassManager = classManager;
    }

    public bool PerformSanityChecks()
    {
        foreach (var @class in ClassManager.Classes)
        {
            if (@class.MaxHitPointGainPerLevel < @class.MinHitPointGainPerLevel)
                Logger.LogWarning("Class {name} max hp per level < min hp per level", @class.Name);
            if (@class.ResourceKinds == null || !@class.ResourceKinds.Any())
                Logger.LogWarning("Class {name} doesn't have any allowed resources", @class.Name);
            else
            {
                foreach (var abilityUsage in @class.AvailableAbilities)
                {
                    foreach(var abilityResourceCost in abilityUsage.ResourceCosts)
                    {
                        if (!@class.ResourceKinds.Contains(abilityResourceCost.ResourceKind))
                            Logger.LogWarning("Class {name} is allowed to use ability {abilityName} [resource:{resource}] but doesn't have access to that resource", @class.DisplayName, abilityUsage.Name, abilityResourceCost.ResourceKind);
                    }
                }
            }
        }
        return false;
    }
}
