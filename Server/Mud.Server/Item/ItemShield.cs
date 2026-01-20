using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemShieldBlueprint), typeof(ItemShieldData))]
public class ItemShield : ItemBase, IItemShield
{
    public ItemShield(ILogger<ItemShield> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemShieldBlueprint blueprint, string source, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, source, containedInto);

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
            Armor = RandomManager.Range(blueprint.Armor * 95 / 100, blueprint.Armor * 105 / 100);
        else
            Armor = blueprint.Armor;
    }

    public void Initialize(Guid guid, ItemShieldBlueprint blueprint, ItemShieldData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Armor = itemData.Armor;
    }

    #region IItemShield

    public int Armor { get; private set; }

    #region ItemBase

    public override ItemShieldData MapItemData()
    {
        return new ItemShieldData
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
            Armor = Armor
        };
    }

    #endregion

    #endregion
}
