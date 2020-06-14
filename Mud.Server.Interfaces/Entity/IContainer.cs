using Mud.Server.Interfaces.Item;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Entity
{
    public interface IContainer : IEntity
    {
        IEnumerable<IItem> Content { get; }

        bool PutInContainer(IItem obj);
        bool GetFromContainer(IItem obj);
    }
}
