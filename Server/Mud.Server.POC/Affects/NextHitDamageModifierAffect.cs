using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.POC.Affects;

[Affect("NextHitDamageModifier", typeof(NextHitDamageModifierAffectData))]
public class NextHitDamageModifierAffect : ICharacterHitDamageModifierAffect
{
    public int Modifier { get; set; }

    public void Initialize(NextHitDamageModifierAffectData data)
    {
        Modifier = data.Modifier;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%adds %y%{0} %c%damage to next hit%x%", Modifier);
    }

    public AffectDataBase MapAffectData()
    {
        return new NextHitDamageModifierAffectData { Modifier = Modifier };
    }

    public (int modifiedDamage, bool wearOff) ModifyDamage(ICharacter? source, SchoolTypes damageType, int damage)
    {
        int modifiedDamage = damage + Modifier;
        return (modifiedDamage, true);
    }
}
