using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IEntity : IActor
    {
        string Name { get; }

        IEnumerable<IAura> Auras { get; }
        IAura GetAura(IAbility ability);
        IAura GetAura(string abilityName);
        bool RemoveAura(IAura aura, bool recompute);

        bool Recompute();
    }
}
