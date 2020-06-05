using System;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IEntity : IActor
    {
        string Name { get; }

        IEnumerable<IAura> Auras { get; }
        IAura GetAura(string abilityName);
        bool RemoveAura(IAura aura, bool recompute);
        bool RemoveAuras(Func<IAura, bool> filterFunc, bool recompute);

        bool Recompute();
    }
}
