using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Numerics;
using System.Text;

namespace Mud.Server.POC.Affects;

[Affect("AbsorbDamage", typeof(AbsorbDamageAffectData))]
public class AbsorbDamageAffect : ICharacterDamageModifierAffect
{
    public int RemainingAbsorb { get; set; }

    public void Initialize(AbsorbDamageAffectData data)
    {
        RemainingAbsorb = data.RemainingAbsorb;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%absorbs %y%{0} %c%damage%x%", RemainingAbsorb);
    }

    public AffectDataBase MapAffectData()
        => new AbsorbDamageAffectData
        {
            RemainingAbsorb = RemainingAbsorb,
        };

    public DamageModifierAffectResult ModifyDamage(ICharacter? source, ICharacter victim, SchoolTypes damageType, DamageSources damageSource, int damage)
    {
        if (damage > 1)
        {
            if (damage >= RemainingAbsorb)
            {
                var modifiedDamage = damage - RemainingAbsorb;
                RemainingAbsorb = 0;
                return new DamageModifierAffectResult
                {
                    ModifiedDamage = modifiedDamage,
                    WornOff = true,
                    Action = DamageModifierAffectAction.DamagePartiallyAbsorbed,
                };
            }
            else
            {
                var modifiedDamage = 0;
                RemainingAbsorb -= damage;
                return new DamageModifierAffectResult
                {
                    ModifiedDamage = modifiedDamage,
                    WornOff = false,
                    Action = DamageModifierAffectAction.DamageFullyAbsorbed,
                };
            }
        }
        return new DamageModifierAffectResult
        {
            ModifiedDamage = damage,
            WornOff = false,
            Action = DamageModifierAffectAction.Nop,
        };
    }
}
