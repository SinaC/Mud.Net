using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer : IEntity
    {
        IReadOnlyCollection<IItem> Content { get; }

        bool PutInContainer(IItem obj);
        bool GetFromContainer(IItem obj);
    }
}
