using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Item;

public interface IItemPortal : IItemCloseable
{
    void Initialize(Guid guid, ItemPortalBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemPortalBlueprint blueprint, ItemPortalData itemData, IContainer containedInto);

    IRoom? Destination { get; }
    PortalFlags PortalFlags { get; }
    int MaxChargeCount { get; }
    int CurrentChargeCount { get; }

    bool HasChargeLeft();
    void ChangeDestination(IRoom? destination);
    void Use();
    void SetCharge(int current, int max);
}
