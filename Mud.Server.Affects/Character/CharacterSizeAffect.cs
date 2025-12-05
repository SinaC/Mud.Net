using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("CharacterSizeAffect", typeof(CharacterSizeAffectData))]
public class CharacterSizeAffect : ICharacterSizeAffect
{
    public Sizes Value { get; set; }

    public CharacterSizeAffect()
    {
    }

    public CharacterSizeAffect(CharacterSizeAffectData data)
    {
        Value = data.Value;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%size %c%by setting to %y%{0}%x%", Value);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterSizeAffectData
        {
            Value = Value
        };
    }
}
