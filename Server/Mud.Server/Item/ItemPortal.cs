using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemPortalBlueprint), typeof(ItemPortalData))]
public class ItemPortal : ItemBase, IItemPortal
{
    private const int InfiniteChargeCount = -1;
    private const int NoDestinationRoomId = -1;

    private IRoomManager RoomManager { get; }

    public ItemPortal(ILogger<ItemPortal> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
        RoomManager = roomManager;
    }

    public void Initialize(Guid guid, ItemPortalBlueprint blueprint, string source, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, source, containedInto);

        Destination = FindDestination(blueprint);
        KeyId = blueprint.Key;
        PortalFlags = blueprint.PortalFlags;
        MaxChargeCount = blueprint.MaxChargeCount;
        CurrentChargeCount = blueprint.CurrentChargeCount;
    }

    public void Initialize(Guid guid, ItemPortalBlueprint blueprint, ItemPortalData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Destination = FindDestination(itemData);
        KeyId = blueprint.Key;
        PortalFlags = itemData.PortalFlags;
        MaxChargeCount = itemData.MaxChargeCount;
        CurrentChargeCount = itemData.CurrentChargeCount;
    }

    private IRoom? FindDestination(ItemPortalBlueprint portalBlueprint)
    {
        IRoom? destination = null;
        if (portalBlueprint.Destination != NoDestinationRoomId)
        {
            destination = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
            if (destination == null)
            {
                destination = RoomManager.DefaultRecallRoom;
                Logger.LogError("World.AddItem: PortalBlueprint {blueprintId} unknown destination {blueprintDestinationId} setting to recall {defaultRecallRoomId}", portalBlueprint.Id, portalBlueprint.Destination, destination.Blueprint.Id);
            }
        }
        return destination;
    }

    private IRoom? FindDestination(ItemPortalData itemPortalData)
    {
        IRoom? destination = null;
        if (itemPortalData != null && itemPortalData.DestinationRoomId != NoDestinationRoomId)
        {
            destination = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == itemPortalData.DestinationRoomId);
            if (destination == null)
            {
                destination = RoomManager.DefaultRecallRoom;
                Logger.LogError("World.AddItem: ItemPortalData unknown destination {destinationRoomId} setting to recall {defaultRecallRoomId}", itemPortalData.DestinationRoomId, destination.Blueprint.Id);
            }
        }
        return destination;
    }

    #region IItemPortal

    #region IItemCloseable

    public int KeyId { get; private set; }

    public bool IsCloseable => !PortalFlags.HasFlag(PortalFlags.NoClose);
    public bool IsLockable => !PortalFlags.HasFlag(PortalFlags.NoLock) && KeyId > 0;
    public bool IsClosed => PortalFlags.HasFlag(PortalFlags.Closed);
    public bool IsLocked => PortalFlags.HasFlag(PortalFlags.Locked);
    public bool IsPickProof => PortalFlags.HasFlag(PortalFlags.PickProof);
    public bool IsEasy => PortalFlags.HasFlag(PortalFlags.Easy);
    public bool IsHard => PortalFlags.HasFlag(PortalFlags.Hard);

    public void Open()
    {
        RemoveFlags(PortalFlags.Closed);
    }

    public void Close()
    {
        if (IsCloseable)
            AddFlags(PortalFlags.Closed);
    }

    public void Unlock()
    {
        RemoveFlags(PortalFlags.Locked);
    }

    public void Lock()
    {
        if (IsLockable && IsClosed)
            AddFlags(PortalFlags.Locked);
    }

    #endregion

    public IRoom? Destination { get; protected set; }

    public PortalFlags PortalFlags { get; protected set; }

    public int MaxChargeCount { get; protected set; }

    public int CurrentChargeCount { get; protected set; }

    public bool HasChargeLeft()
    {
        if (MaxChargeCount == InfiniteChargeCount)
            return true;
        return CurrentChargeCount > 0;
    }

    public void ChangeDestination(IRoom? destination)
    {
        Destination = destination;
    }

    public void Use()
    {
        if (MaxChargeCount == InfiniteChargeCount)
            return;
        CurrentChargeCount = Math.Max(0, CurrentChargeCount - 1);
    }

    public void SetCharge(int current, int max)
    {
        MaxChargeCount = Math.Max(1, max);
        CurrentChargeCount = Math.Clamp(current, 1, MaxChargeCount);
    }

    #endregion

    #region ItemBase

    public override ItemPortalData MapItemData()
    {
        return new ItemPortalData
        {
            ItemId = Blueprint.Id,
            Source = Source,
            ShortDescription = ShortDescription,
            Description = Description,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(), // Current will be recompute with auras
            Auras = MapAuraData(),
            DestinationRoomId = Destination?.Blueprint?.Id ?? -1,
            PortalFlags = PortalFlags,
            MaxChargeCount = MaxChargeCount,
            CurrentChargeCount = CurrentChargeCount,
        };
    }

    #endregion

    private void AddFlags(PortalFlags flags)
    {
        PortalFlags |= flags;
    }

    private void RemoveFlags(PortalFlags flags)
    {
        PortalFlags &= ~flags;
    }
}
