using System.Collections.Generic;

namespace Mud.POC.Affects
{
    public interface IEntity
    {
        bool IsValid { get; }
        string Name { get; }
        IEnumerable<IAura> Auras { get; }

        void Recompute();

        void AddAura(IAura aura);
    }
}
