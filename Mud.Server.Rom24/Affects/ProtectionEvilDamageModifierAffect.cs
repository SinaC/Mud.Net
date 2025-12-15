using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects
{
    [Affect("ProtectEvil", typeof(ProtectionEvilDamageModifierAffectData))]
    public class ProtectionEvilDamageModifierAffect : ICharacterDamageModifierAffect, ICustomAffect
    {
        public void Initialize(AffectDataBase data)
        {
            // NOP
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%reduces %y%incoming damage%c% from %y%evil source%c% by %y%25%%x%");
        }

        public AffectDataBase MapAffectData()
        {
            return new ProtectionEvilDamageModifierAffectData();
        }

        public int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage)
        {
            if (damage > 1 && source.IsEvil)
                damage -= damage / 4;
            return damage;
        }
    }
}
