using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Item
{
    public interface IItemCloseable : IItem
    {
        int KeyId { get; }

        bool IsCloseable { get; }
        bool IsLockable { get; }
        bool IsClosed { get; }
        bool IsLocked { get; }
        bool IsPickProof { get; }

        void Open();
        void Close();
        void Unlock();
        void Lock();
    }
}
