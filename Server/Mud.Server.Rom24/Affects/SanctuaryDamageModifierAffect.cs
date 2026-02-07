using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[AffectNoData("Sanctuary")]
public class SanctuaryDamageModifierAffect : NoAffectDataAffectBase, ICharacterDamageModifierAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%reduces %y%Incoming damage%c% by %y%50%%x%");
    }

    public DamageModifierAffectResult ModifyDamage(ICharacter? source, ICharacter victim, SchoolTypes damageType, DamageSources damageSource, int damage)
    {
        if (damage > 1)
            return new DamageModifierAffectResult
            {
                ModifiedDamage = damage / 2,
                WornOff = false,
                Action = DamageModifierAffectAction.DamageDecreased,
            };
        return new DamageModifierAffectResult
        {
            ModifiedDamage = damage,
            WornOff = false,
            Action = DamageModifierAffectAction.Nop,
        };
    }
}
