using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrCharacterDebuffSpellBase : ItemOrOffensiveSpellBase, IAbilityCharacterBuff, IAbilityItemBuff
    {
        public ItemOrCharacterDebuffSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region IAbility

        public override AbilityEffects Effects => AbilityEffects.Debuff;

        #endregion

        #region ICharacterBuff

        public abstract string CharacterWearOffMessage { get; }

        #endregion

        #region IItemBuff

        public abstract string ItemWearOffMessage { get; }

        #endregion
    }
}
