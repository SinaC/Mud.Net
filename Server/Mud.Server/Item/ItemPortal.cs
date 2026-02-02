using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Flags;
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
    private IFlagsManager FlagsManager { get; }

    public ItemPortal(ILogger<ItemPortal> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager, IFlagsManager flagsManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
        RoomManager = roomManager;
        FlagsManager = flagsManager;
        PortalFlags = new PortalFlags();
    }

    public void Initialize(Guid guid, ItemPortalBlueprint blueprint, string source, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, source, containedInto);

        Destination = FindDestination(blueprint);
        KeyId = blueprint.Key;
        PortalFlags = blueprint.PortalFlags;
        FlagsManager.CheckFlags(PortalFlags);
        MaxChargeCount = blueprint.MaxChargeCount;
        CurrentChargeCount = blueprint.CurrentChargeCount;
    }

    public void Initialize(Guid guid, ItemPortalBlueprint blueprint, ItemPortalData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Destination = FindDestination(itemData);
        KeyId = blueprint.Key;
        PortalFlags = NewAndCopyAndSet(() => new PortalFlags(), new PortalFlags(itemData.PortalFlags), null);
        FlagsManager.CheckFlags(PortalFlags); 
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

    public bool IsCloseable => !PortalFlags.IsSet("NoClose");
    public bool IsLockable => !PortalFlags.IsSet("NoLock") && KeyId > 0;
    public bool IsClosed => PortalFlags.IsSet("Closed");
    public bool IsLocked => PortalFlags.IsSet("Locked");
    public bool IsPickProof => PortalFlags.IsSet("PickProof");
    public bool IsEasy => PortalFlags.IsSet("Easy");
    public bool IsHard => PortalFlags.IsSet("Hard");

    public void Open()
    {
        PortalFlags.Unset("Closed");
    }

    public void Close()
    {
        if (IsCloseable)
            PortalFlags.Set("Closed");
    }

    public void Unlock()
    {
        PortalFlags.Unset("Locked");
    }

    public void Lock()
    {
        if (IsLockable && IsClosed)
            PortalFlags.Set("Locked");
    }

    #endregion

    public IRoom? Destination { get; protected set; }

    public IPortalFlags PortalFlags { get; protected set; }

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
            PortalFlags = PortalFlags.Serialize(),
            MaxChargeCount = MaxChargeCount,
            CurrentChargeCount = CurrentChargeCount,
        };
    }

    #endregion
}
