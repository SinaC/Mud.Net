using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer : IEntity
    {
        IEnumerable<IItem> Content { get; }

        // TODO: put on or put in    see CONT_PUT_ON

        bool PutInContainer(IItem obj);
        bool GetFromContainer(IItem obj);
    }
}
