using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemArmorBlueprint), typeof(ItemArmorData))]
public class ItemArmor : ItemBase, IItemArmor
{
    public ItemArmor(ILogger<ItemArmor> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemArmorBlueprint blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
        {
            Bash = RandomManager.Range(blueprint.Bash * 95 / 100, blueprint.Bash * 105 / 100);
            Pierce = RandomManager.Range(blueprint.Pierce * 95 / 100, blueprint.Pierce * 105 / 100);
            Slash = RandomManager.Range(blueprint.Slash * 95 / 100, blueprint.Slash * 105 / 100);
            Exotic = RandomManager.Range(blueprint.Exotic * 95 / 100, blueprint.Exotic * 105 / 100);
        }
        else
        {
            Bash = blueprint.Bash;
            Pierce = blueprint.Pierce;
            Slash = blueprint.Slash;
            Exotic = blueprint.Exotic;
        }
    }

    public void Initialize(Guid guid, ItemArmorBlueprint blueprint, ItemArmorData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Bash = itemData.Bash;
        Pierce = itemData.Pierce;
        Slash = itemData.Slash;
        Exotic = itemData.Exotic;
    }

    #region IItemArmor

    public int Bash { get; private set; }
    public int Pierce { get; private set; }
    public int Slash { get; private set; }
    public int Exotic { get; private set; }

    public void SetArmor(int bash, int pierce, int slash, int exotic)
    {
        Bash = bash;
        Pierce = pierce;
        Slash = slash;
        Exotic = exotic;
    }

    #region ItemBase

    public override ItemArmorData MapItemData()
    {
        return new ItemArmorData
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
            Bash = Bash,
            Pierce = Pierce,
            Slash = Slash,
            Exotic = Exotic,
        };
    }

    #endregion

    #endregion
}
