using System.Collections.Generic;

namespace Mud.Server
{
    public interface IClassManager
    {
        IEnumerable<IClass> Classes { get; }

        IClass this[string name] { get; }
    }
}
