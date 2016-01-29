using System.Collections.Generic;

namespace Mud.Server
{
    public interface IClassManager
    {
        IReadOnlyCollection<IClass> Classes { get; }

        IClass this[string name] { get; }
    }
}
