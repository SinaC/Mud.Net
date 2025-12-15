using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects
{
    [Affect("Sanctuary", typeof(SanctuaryDamageModifierAffectData))]
    public class SanctuaryDamageModifierAffect : ICharacterDamageModifierAffect, ICustomAffect
    {
        public void Initialize(AffectDataBase data)
        {
            // NOP
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%reduces %y%incoming damage%c% by %y%50%%x%");
        }

        public AffectDataBase MapAffectData()
        {
            return new SanctuaryDamageModifierAffectData();
        }

        public int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage)
        {
            return damage > 1 ? damage / 2 : damage;
        }
    }
}
