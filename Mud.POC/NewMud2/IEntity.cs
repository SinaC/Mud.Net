using System.Collections.Generic;

namespace Mud.POC.NewMud2
{
    public interface IEntity
    {
        IWorld World { get; }

        string Name { get; }

        IEnumerable<IAura> Auras { get; }

        void AddAura(IAura aura);
        void RemoveAura(IAura aura);
    }
}
