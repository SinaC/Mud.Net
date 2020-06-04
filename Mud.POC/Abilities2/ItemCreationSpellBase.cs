using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class ItemCreationSpellBase : NoTargetSpellBase
    {
        protected IItemManager ItemManager { get; }
        protected ISettings Settings { get; }

        protected ItemCreationSpellBase(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
            : base(randomManager, wiznet)
        {
            ItemManager = itemManager;
            Settings = settings;
        }
    }
}
