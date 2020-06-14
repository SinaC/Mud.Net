namespace Mud.Server.Interfaces
{
    public interface ICloseable
    {
        int KeyId { get; }

        bool IsCloseable { get; }
        bool IsLockable { get; }
        bool IsClosed { get; }
        bool IsLocked { get; }
        bool IsPickProof { get; }
        bool IsEasy { get; }
        bool IsHard { get; }

        void Open();
        void Close();
        void Unlock();
        void Lock();
    }
}
