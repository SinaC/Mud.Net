using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrCharacterBuffSpellBase : ItemOrDefensiveSpellBase, IAbilityCharacterBuff, IAbilityItemBuff
    {
        protected ItemOrCharacterBuffSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region IAbility

        public override AbilityFlags Flags => AbilityFlags.CanBeDispelled;

        #endregion

        #region ICharacterBuff

        public abstract string CharacterWearOffMessage { get; }

        #endregion

        #region IItemBuff

        public abstract string ItemWearOffMessage { get; }

        #endregion
    }
}
