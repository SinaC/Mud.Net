using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell
{
    public abstract class DamageSpellBase : OffensiveSpellBase
    {
        protected bool SavesSpellResult { get; private set; }
        protected DamageResults DamageResult { get; private set; }

        protected DamageSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            //
            int damage = DamageValue;
            SavesSpellResult = Victim.SavesSpell(Level, DamageType);
            if (SavesSpellResult)
                damage /= 2;
            DamageResult = Victim.AbilityDamage(Caster, damage, DamageType, DamageNoun, true);
        }

        protected abstract SchoolTypes DamageType { get; }
        protected abstract int DamageValue { get; }
        protected abstract string DamageNoun { get; }
    }
}
