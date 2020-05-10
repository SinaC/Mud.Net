using Mud.Domain;

namespace Mud.Server.Item
{
    public interface IItemPortal : IItem
    {
        IRoom Destination { get; }
        PortalFlags PortalFlags { get; }
        int MaxChargeCount { get; }
        int CurrentChargeCount { get; }

        bool IsClosed { get; }
        bool IsLocked { get; }

        void Open();
        void Close();
        void Unlock();
        void Lock();

        bool HasChargeLeft();
        void ChangeDestination(IRoom destination);
        void Use();
        void SetCharge(int current, int max);
    }
}
