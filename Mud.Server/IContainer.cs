using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer : IEntity // TODO: must really inherits from IEntity ?
    {
        IEnumerable<IItem> Content { get; }

        int MaxWeight { get; }
        int MaxWeightPerItem { get; }

        bool PutInContainer(IItem obj);
        bool GetFromContainer(IItem obj);
    }
}
