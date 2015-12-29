using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer : IEntity
    {
        IReadOnlyCollection<IItem> Content { get; }

        bool Put(IItem obj);
        bool Get(IItem obj);
    }
}
