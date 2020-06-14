using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IContainer : IEntity // TODO: must really inherits from IEntity ?
    {
        IEnumerable<IItem> Content { get; }

        bool PutInContainer(IItem obj);
        bool GetFromContainer(IItem obj);
    }
}
