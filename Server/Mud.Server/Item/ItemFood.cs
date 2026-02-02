using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemFoodBlueprint), typeof(ItemFoodData))]
public class ItemFood : ItemBase, IItemFood
{
    public ItemFood(ILogger<ItemFood> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
       : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemFoodBlueprint blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

        FullHours = blueprint.FullHours;
        HungerHours = blueprint.HungerHours;
        IsPoisoned = blueprint.IsPoisoned;
    }

    public void Initialize(Guid guid, ItemFoodBlueprint blueprint, ItemFoodData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        FullHours = data.FullHours;
        HungerHours = data.HungerHours;
        IsPoisoned = data.IsPoisoned;
    }

    #region IItemFood

    #region IItemPoisonable

    public bool IsPoisoned { get; protected set; }

    public void Poison()
    {
        IsPoisoned = true;
    }

    public void Cure()
    {
        IsPoisoned = false;
    }

    #endregion

    public int FullHours { get; protected set; }

    public int HungerHours { get; protected set; }

    public void SetHours(int fullHours, int hungerHours) // TODO: should be replaced with ctor parameters
    {
        FullHours = fullHours;
        HungerHours = hungerHours;
    }

    #endregion

    #region ItemBase

    public override ItemFoodData MapItemData()
    {
        return new ItemFoodData
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
            FullHours = FullHours,
            HungerHours = HungerHours,
            IsPoisoned = IsPoisoned
        };
    }

    #endregion
}
