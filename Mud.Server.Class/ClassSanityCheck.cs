using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
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
        foreach (IClass c in ClassManager.Classes)
        {
            if (c.MaxHitPointGainPerLevel < c.MinHitPointGainPerLevel)
                Logger.LogWarning("Class {name} max hp per level < min hp per level", c.Name);
            if (c.ResourceKinds == null || !c.ResourceKinds.Any())
                Logger.LogWarning("Class {name} doesn't have any allowed resources", c.Name);
            else
            {
                foreach (IAbilityUsage abilityUsage in c.Abilities)
                    if (abilityUsage.ResourceKind.HasValue && !c.ResourceKinds.Contains(abilityUsage.ResourceKind.Value))
                        Logger.LogWarning("Class {name} is allowed to use ability {abilityName} [resource:{resource}] but doesn't have access to that resource", c.DisplayName, abilityUsage.Name, abilityUsage.ResourceKind);
            }
        }
        return false;
    }
}
