using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[AffectNoData("ProtectGood")]
public class ProtectionGoodDamageModifierAffect : NoAffectDataAffectBase, ICharacterDamageModifierAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%reduces %y%Incoming damage%c% from %y%Good source%c% by %y%25%%x%");
    }

    public DamageModifierAffectResult ModifyDamage(ICharacter? source, ICharacter victim, SchoolTypes damageType, DamageSources damageSource, int damage)
    {
        if (damage > 1 && source?.IsGood == true)
            return new DamageModifierAffectResult
            {
                ModifiedDamage = 3 * damage / 4,
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
