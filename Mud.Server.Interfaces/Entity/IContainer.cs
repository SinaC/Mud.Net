using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Entity;

public interface IContainer : IEntity
{
    IEnumerable<IItem> Content { get; }

    bool PutInContainer(IItem obj);
    bool GetFromContainer(IItem obj);
}
