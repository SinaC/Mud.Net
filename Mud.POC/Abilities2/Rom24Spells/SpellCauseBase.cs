using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public abstract class SpellCauseBase : CharacterDamageSpellBase
    {
        public SpellCauseBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Harm;

        protected override string DamageNoun => "spell";
    }
}
