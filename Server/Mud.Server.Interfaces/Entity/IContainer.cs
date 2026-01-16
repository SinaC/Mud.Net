using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Entity;

public interface IContainer : IEntity
{
    IEnumerable<IItem> Content { get; }

    // these methods should only be called in ItemBase
    bool PutInContainer(IItem obj);
    bool GetFromContainer(IItem obj);
}
