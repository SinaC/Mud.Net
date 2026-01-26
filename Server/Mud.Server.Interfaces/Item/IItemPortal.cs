using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Item;

public interface IItemPortal : IItemCloseable
{
    IRoom? Destination { get; }
    IPortalFlags PortalFlags { get; }
    int MaxChargeCount { get; }
    int CurrentChargeCount { get; }

    bool HasChargeLeft();
    void ChangeDestination(IRoom? destination);
    void Use();
    void SetCharge(int current, int max);
}
