using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("AggroModifier", typeof(CharacterAggroModifierAffectData))]
public class CharacterAggroModifierAffect : ICharacterAggroModifierAffect
{
    public int MultiplierInPercent { get; set; }

    public void Initialize(CharacterAggroModifierAffectData data)
    {
        MultiplierInPercent = data.MultiplierInPercent;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%generates %y%{0}%% %c%more aggro%x%", MultiplierInPercent);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterAggroModifierAffectData { MultiplierInPercent = MultiplierInPercent };
    }
}
