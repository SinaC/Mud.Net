using System.Collections.Generic;

namespace Mud.Server.Interfaces
{
    public interface IClassManager.Class
    {
        IEnumerable<IClass> Classes { get; }

        IClass this[string name] { get; }
    }
}
