using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[AffectNoData("Sanctuary")]
public class SanctuaryDamageModifierAffect : NoAffectDataAffectBase, ICharacterDamageModifierAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%reduces %y%incoming damage%c% by %y%50%%x%");
    }

    public int ModifyDamage(ICharacter? source, ICharacter victim, SchoolTypes damageType, DamageSources damageSource, int damage)
        => damage > 1
            ? damage / 2
            : damage;
}
