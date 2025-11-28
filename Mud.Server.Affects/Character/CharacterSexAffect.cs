using Mud.Domain;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("CharacterSexAffect", typeof(CharacterSexAffectData))]
public class CharacterSexAffect : ICharacterSexAffect
{
    public Sex Value { get; set; }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%sex %c%by setting to %y%{0}%x%", Value);
    }

    public CharacterSexAffect()
    {
    }

    public CharacterSexAffect(CharacterSexAffectData data)
    {
        Value = data.Value;
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterSexAffectData
        {
            Value = Value
        };
    }
}
