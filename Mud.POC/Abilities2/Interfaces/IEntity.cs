using System.Collections.Generic;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IEntity
    {
        string Name { get; }

        IEnumerable<IAura> Auras { get; }
        IAura GetAura(IAbility ability);
        IAura GetAura(string abilityName);
        bool RemoveAura(IAura aura, bool recompute);

        bool Recompute();
    }
}
