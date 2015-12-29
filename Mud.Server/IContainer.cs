using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer
    {
        IReadOnlyCollection<IObject> ObjectsInContainer { get; }

        bool Put(IObject obj);
        bool Get(IObject obj);
    }
}
