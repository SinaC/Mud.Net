using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects
{
    [Affect(AffectName, typeof(NoAffectData))]
    public class SanctuaryDamageModifierAffect : ICharacterDamageModifierAffect
    {
        private const string AffectName = "Sanctuary";

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%reduces %y%incoming damage%c% by %y%50%%x%");
        }

        public AffectDataBase MapAffectData()
        {
            return new NoAffectData { AffectName = AffectName };
        }

        public int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage)
        {
            return damage > 1 ? damage / 2 : damage;
        }
    }
}
