using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain.SerializationData.Avatar;
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

[Item(typeof(ItemContainerBlueprint), typeof(ItemContainerData))]
public class ItemContainer : ItemBase, IItemContainer
{
    private readonly List<IItem> _content;

    private IItemManager ItemManager { get; }
    private IFlagsManager FlagsManager { get; }

    public ItemContainer(ILogger<ItemContainer> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager, IFlagsManager flagsManager)
            : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
        ItemManager = itemManager;
        FlagsManager = flagsManager;
        _content = [];
        ContainerFlags = new ContainerFlags();
    }

    public void Initialize(Guid guid, ItemContainerBlueprint blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

        MaxWeight = blueprint.MaxWeight;
        ContainerFlags = blueprint.ContainerFlags;
        FlagsManager.CheckFlags(ContainerFlags);
        KeyId = blueprint.Key;
        MaxWeightPerItem = blueprint.MaxWeightPerItem;
        WeightMultiplier = blueprint.WeightMultiplier;
    }

    public void Initialize(Guid guid, ItemContainerBlueprint blueprint, ItemContainerData itemContainerData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemContainerData, containedInto);
        
        MaxWeight = itemContainerData.MaxWeight;
        ContainerFlags = NewAndCopyAndSet(() => new ContainerFlags(), new ContainerFlags(itemContainerData.ContainerFlags), null);
        FlagsManager.CheckFlags(ContainerFlags);
        KeyId = blueprint.Key;
        MaxWeightPerItem = itemContainerData.MaxWeightPerItem;
        WeightMultiplier = blueprint.WeightMultiplier;
        if (itemContainerData.Contains?.Length > 0)
        {
            foreach (var itemData in itemContainerData.Contains)
                ItemManager.AddItem(Guid.NewGuid(), itemData, this);
        }
    }

    #region IItemContainer

    #region IItemCloseable

    public int KeyId { get; private set; }

    public bool IsCloseable => !ContainerFlags.IsSet("NoClose");
    public bool IsLockable => !ContainerFlags.IsSet("NoLock") && KeyId > 0;
    public bool IsClosed => ContainerFlags.IsSet("Closed");
    public bool IsLocked => ContainerFlags.IsSet("Locked");
    public bool IsPickProof => ContainerFlags.IsSet("PickProof");
    public bool IsEasy => ContainerFlags.IsSet("Easy");
    public bool IsHard => ContainerFlags.IsSet("Hard");

    public void Open()
    {
        ContainerFlags.Unset("Closed");
    }

    public void Close()
    {
        if (IsCloseable)
            ContainerFlags.Set("Closed");
    }

    public void Unlock()
    {
        ContainerFlags.Unset("Locked");
    }

    public void Lock()
    {
        if (IsLockable && IsClosed)
            ContainerFlags.Set("Locked");
    }

    #endregion

    #region IItem

    public override int TotalWeight => Weight + _content.Sum(x => x.TotalWeight) * WeightMultiplier;

    public override int CarryCount => _content.Sum(x => x.CarryCount);

    #endregion

    #region IContainer

    public IEnumerable<IItem> Content => _content;

    public int MaxItems { get; protected set; }
    public int MaxWeight { get; protected set; }
    public int MaxWeightPerItem { get; protected set; }

    public bool PutInContainer(IItem obj)
    {
        //if (obj.ContainedInto != null)
        //{
        //    Logger.LogError("PutInContainer: {0} is already in container {1}.", obj.DebugName, obj.ContainedInto.DebugName);
        //    return false;
        //}
        _content.Insert(0, obj);
        return true;
    }

    public bool GetFromContainer(IItem obj)
    {
        bool removed = _content.Remove(obj);
        return removed;
    }

    #endregion

    public IContainerFlags ContainerFlags { get; protected set; }
    public int WeightMultiplier { get; private set; } // percentage

    public void SetCustomValues(int level, int maxWeight, int maxWeightPerItem) // TODO: should be remove once a system to create item with custom values is implemented
    {
        Level = level;
        MaxWeight = maxWeight;
        MaxWeightPerItem = maxWeightPerItem;
    }

    #endregion

    #region ItemBase

    public override void OnRemoved(IRoom nullRoom)
    {
        base.OnRemoved(nullRoom);

        _content.Clear();
    }

    public override ItemContainerData MapItemData()
    {
        return new ItemContainerData
        {
            ItemId = Blueprint.Id,
            Source = Source,
            ShortDescription = ShortDescription,
            Description = Description,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            MaxWeight = MaxWeight,
            ContainerFlags = ContainerFlags.Serialize(),
            MaxWeightPerItem = MaxWeightPerItem,
            Contains = MapContent(),
        };
    }

    #endregion

    private ItemData[] MapContent()
    {
        if (Content.Any())
            return Content.Select(x => x.MapItemData()).ToArray();
        return [];
    }
}
