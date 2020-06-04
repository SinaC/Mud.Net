using Mud.Server.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2
{
    public abstract class DamageSpellBase : OffensiveSpellBase
    {
        protected bool SavesSpellResult { get; private set; }
        protected DamageResults DamageResult { get; private set; }

        protected DamageSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            //
            int damage = DamageValue;
            SavesSpellResult = Victim.SavesSpell(Level, DamageType);
            if (SavesSpellResult)
                damage /= 2;
            DamageResult = Victim.AbilityDamage(Caster, this, damage, DamageType, DamageNoun, true);
        }

        protected abstract SchoolTypes DamageType { get; }
        protected abstract int DamageValue { get; }
        protected abstract string DamageNoun { get; }
    }
}
