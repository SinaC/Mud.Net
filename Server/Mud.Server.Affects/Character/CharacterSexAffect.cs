using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
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
        sb.AppendFormat("%c%modifies %y%Sex %c%by setting to %y%{0}%x%", Value.ToString().ToPascalCase());
    }

    public void Initialize(CharacterSexAffectData data)
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
