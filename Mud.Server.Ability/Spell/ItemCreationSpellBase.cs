using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class ItemCreationSpellBase : NoTargetSpellBase
{
    protected IWiznet Wiznet { get; }
    protected IItemManager ItemManager { get; }

    protected ItemCreationSpellBase(ILogger<ItemCreationSpellBase> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager)
        : base(logger, randomManager)
    {
        Wiznet = wiznet;
        ItemManager = itemManager;
    }
}
