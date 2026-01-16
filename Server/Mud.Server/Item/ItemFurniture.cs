using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Random;
using System.Text;

namespace Mud.Server.Item;

[Item(typeof(ItemFurnitureBlueprint), typeof(ItemData))]
public class ItemFurniture : ItemBase, IItemFurniture
{
    public ItemFurniture(ILogger<ItemFurniture> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemFurnitureBlueprint blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        MaxPeople = blueprint.MaxPeople;
        MaxWeight = blueprint.MaxWeight;
        FurnitureActions = blueprint.FurnitureActions;
        FurniturePlacePreposition = blueprint.FurniturePlacePreposition;
        HealBonus = blueprint.HealBonus;
        ResourceBonus = blueprint.ResourceBonus;
    }

    public void Initialize(Guid guid, ItemFurnitureBlueprint blueprint, ItemData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        MaxPeople = blueprint.MaxPeople;
        MaxWeight = blueprint.MaxWeight;
        FurnitureActions = blueprint.FurnitureActions;
        FurniturePlacePreposition = blueprint.FurniturePlacePreposition;
        HealBonus = blueprint.HealBonus;
        ResourceBonus = blueprint.ResourceBonus;
    }

    #region IItemFurniture

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemFurniture>();

    #endregion

    // Count number of people in room using 'this' as furniture
    public IEnumerable<ICharacter>? People
        => (ContainedInto as IRoom)?.People?.Where(x => x.Furniture == this);

    public int MaxPeople { get; private set; }
    public int MaxWeight { get; private set; }
    public FurnitureActions FurnitureActions { get; private set; }
    public FurniturePlacePrepositions FurniturePlacePreposition { get; private set; }
    public int HealBonus { get; private set; }
    public int ResourceBonus { get; private set; }

    public bool CanStand => FurnitureActions.HasFlag(FurnitureActions.Stand);
    public bool CanSit => FurnitureActions.HasFlag(FurnitureActions.Sit);
    public bool CanRest => FurnitureActions.HasFlag(FurnitureActions.Rest);
    public bool CanSleep => FurnitureActions.HasFlag(FurnitureActions.Sleep);

    public StringBuilder AppendPosition(StringBuilder sb, string verb)
    {
        if (FurniturePlacePreposition == FurniturePlacePrepositions.At)
            sb.AppendFormat(" is {0} at {1}", verb, DisplayName);
        else if (FurniturePlacePreposition == FurniturePlacePrepositions.On)
            sb.AppendFormat(" is {0} on {1}", verb, DisplayName);
        else if (FurniturePlacePreposition == FurniturePlacePrepositions.In)
            sb.AppendFormat(" is {0} in {1}", verb, DisplayName);
        else
            sb.AppendFormat(" is {0} here.", verb);
        return sb;
    }

    #endregion
}
