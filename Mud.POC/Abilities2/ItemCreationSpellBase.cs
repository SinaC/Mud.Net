using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class ItemCreationSpellBase : NoTargetSpellBase
    {
        protected IItemManager ItemManager { get; }
        protected ISettings Settings { get; }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => Enumerable.Empty<IEntity>();

        protected ItemCreationSpellBase(IRandomManager randomManager, IItemManager itemManager, ISettings settings)
            : base(randomManager)
        {
            ItemManager = itemManager;
            Settings = settings;
        }
    }
}
