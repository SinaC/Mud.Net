using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects
{
    [Affect(AffectName, typeof(NoAffectData))]
    public class ProtectionGoodDamageModifierAffect : ICharacterDamageModifierAffect
    {
        private const string AffectName = "ProtectGood";

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%reduces %y%incoming damage%c% from %y%good source%c% by %y%25%%x%");
        }

        public AffectDataBase MapAffectData()
        {
            return new NoAffectData { AffectName = AffectName };
        }

        public int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage)
        {
            if (damage > 1 && source.IsGood)
                damage -= damage / 4;
            return damage;
        }
    }
}
