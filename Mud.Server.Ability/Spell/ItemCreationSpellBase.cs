using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.Ability.Spell;

public abstract class ItemCreationSpellBase : NoTargetSpellBase
{
    protected IWiznet Wiznet { get; }
    protected IItemManager ItemManager { get; }
    protected ISettings Settings { get; }

    protected ItemCreationSpellBase(ILogger<ItemCreationSpellBase> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
        : base(logger, randomManager)
    {
        Wiznet = wiznet;
        ItemManager = itemManager;
        Settings = settings;
    }
}
