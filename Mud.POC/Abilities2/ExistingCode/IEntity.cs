using System;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IEntity : IActor
    {
        bool IsValid { get; }

        string Name { get; }
        IEnumerable<string> Keywords { get; }
        string DisplayName { get; }
        string DebugName { get; }

        IEnumerable<IAura> Auras { get; }
        IAura GetAura(string abilityName);
        bool RemoveAura(IAura aura, bool recompute);
        bool RemoveAuras(Func<IAura, bool> filterFunc, bool recompute);

        bool Recompute();
    }
}
