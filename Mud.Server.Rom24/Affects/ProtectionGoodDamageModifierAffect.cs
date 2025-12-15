using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects
{
    [Affect("ProtectGood", typeof(ProtectionGoodDamageModifierAffectData))]
    public class ProtectionGoodDamageModifierAffect : ICharacterDamageModifierAffect, ICustomAffect
    {
        public void Initialize(AffectDataBase data)
        {
            // NOP
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%reduces %y%incoming damage%c% from %y%good source%c% by %y%25%%x%");
        }

        public AffectDataBase MapAffectData()
        {
            return new ProtectionGoodDamageModifierAffectData();
        }

        public int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage)
        {
            if (damage > 1 && source.IsGood)
                damage -= damage / 4;
            return damage;
        }
    }
}
