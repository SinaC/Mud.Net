using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings;

namespace Mud.Server.Ability.Spell
{
    public abstract class ItemCreationSpellBase : NoTargetSpellBase
    {
        protected IItemManager ItemManager { get; }
        protected ISettings Settings { get; }

        protected ItemCreationSpellBase(IRandomManager randomManager, IItemManager itemManager, ISettings settings)
            : base(randomManager)
        {
            ItemManager = itemManager;
            Settings = settings;
        }
    }
}
