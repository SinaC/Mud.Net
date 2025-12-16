using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[AffectNoData("ProtectGood")]
public class ProtectionGoodDamageModifierAffect : NoAffectDataAffectBase, ICharacterDamageModifierAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%reduces %y%incoming damage%c% from %y%good source%c% by %y%25%%x%");
    }

    public int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage)
        => damage > 1 && source.IsGood
            ? damage -= damage / 4
            : damage;
}
