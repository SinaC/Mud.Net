using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer : IEntity
    {
        IEnumerable<IItem> Content { get; }

        bool PutInContainer(IItem obj);
        bool GetFromContainer(IItem obj);
    }
}
