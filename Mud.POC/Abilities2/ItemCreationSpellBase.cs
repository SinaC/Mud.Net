using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2
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
