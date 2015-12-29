using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer// : IEntity TODO: is this true ?
    {
        IReadOnlyCollection<IItem> Inside { get; }

        bool Put(IItem obj);
        bool Get(IItem obj);
    }
}
