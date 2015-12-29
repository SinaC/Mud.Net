using System.Collections.Generic;

namespace Mud.Server
{
    public interface IContainer
    {
        IReadOnlyCollection<IItem> Inside { get; }

        bool Put(IItem obj);
        bool Get(IItem obj);
    }
}
