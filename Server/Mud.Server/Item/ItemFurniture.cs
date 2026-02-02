using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Text;

namespace Mud.Server.Item;

[Item(typeof(ItemFurnitureBlueprint), typeof(ItemData))]
public class ItemFurniture : ItemBase, IItemFurniture
{
    private IFlagsManager FlagsManager { get; }

    public ItemFurniture(ILogger<ItemFurniture> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager, IFlagsManager flagsManager)
        : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
        FlagsManager = flagsManager;

        FurnitureActions = new FurnitureActions();
    }

    public void Initialize(Guid guid, ItemFurnitureBlueprint blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

        MaxPeople = blueprint.MaxPeople;
        MaxWeight = blueprint.MaxWeight;
        FurnitureActions = blueprint.FurnitureActions;
        FlagsManager.CheckFlags(FurnitureActions);
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
        FlagsManager.CheckFlags(FurnitureActions);
        FurniturePlacePreposition = blueprint.FurniturePlacePreposition;
        HealBonus = blueprint.HealBonus;
        ResourceBonus = blueprint.ResourceBonus;
    }

    #region IItemFurniture

    // Count number of people in room using 'this' as furniture
    public IEnumerable<ICharacter>? People
        => (ContainedInto as IRoom)?.People?.Where(x => x.Furniture == this);

    public int MaxPeople { get; private set; }
    public int MaxWeight { get; private set; }
    public IFurnitureActions FurnitureActions { get; private set; }
    public FurniturePlacePrepositions FurniturePlacePreposition { get; private set; }
    public int HealBonus { get; private set; }
    public int ResourceBonus { get; private set; }

    public bool CanStand => FurnitureActions.IsSet("Stand");
    public bool CanSit => FurnitureActions.IsSet("Sit");
    public bool CanRest => FurnitureActions.IsSet("Rest");
    public bool CanSleep => FurnitureActions.IsSet("Sleep");

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
